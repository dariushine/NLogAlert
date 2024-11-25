using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace NlogAlert;

public class Worker : BackgroundService
{
    private readonly AppSettings _settings;

    private readonly ILogger<Worker> _logger;
    private readonly HttpClient _client;

    public Worker(ILogger<Worker> logger, HttpClient client, IOptions<AppSettings> settings)
    {
        _logger = logger;
        _client = client;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string? currentLogFile = null;
        long lastPosition = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string? latestDirectory = Directory.GetDirectories(_settings.LogDirectory)
                    .OrderByDescending(d => Directory.GetCreationTimeUtc(d))
                    .FirstOrDefault();

                if (latestDirectory != null)
                {
                    string latestLogFilePath = Path.Combine(latestDirectory, _settings.LogFileName);
                    if (currentLogFile == null) currentLogFile = latestLogFilePath;

                    if (File.Exists(currentLogFile))
                    {
                        

                        using (FileStream fs = new FileStream(currentLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            fs.Seek(lastPosition, SeekOrigin.Begin);

                            using (StreamReader reader = new StreamReader(fs))
                            {
                                string? line;
                                while ((line = await reader.ReadLineAsync()) != null)
                                {
                                    if (line.Contains(_settings.SearchPattern))
                                    {
                                        // Call your API here
                                        _logger.LogInformation("Pattern found! Calling API...");
                                        // Replace with your API call logic
                                        CallApi();
                                    }

                                    lastPosition = reader.BaseStream.Position;
                                }
                            }
                        }
                    }

                    if (latestLogFilePath != currentLogFile)
                    {
                        // New log file detected
                        lastPosition = 0;
                        currentLogFile = latestLogFilePath;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while reading: {exception}", ex);
                // Handle exceptions, e.g., log, retry, or notify
                lastPosition = 0;
                currentLogFile = null;
            }

            await Task.Delay(_settings.Delay * 1000, stoppingToken); // 5 minutes delay

        }
    }


            private async void CallApi()
        {
            var obj = new
            {
                Type = _settings.Type,
                Content = string.Format(_settings.Content, DateTime.Now.ToString()) 
            };

            JsonContent content = JsonContent.Create(obj);
            try
            {
                var response = await _client.PostAsync(_settings.ApiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response: {response}", responseString);
            }
            catch (Exception ex)
            {
                _logger.LogError("There was an error during the notification: {exception}", ex);
            }
        }
}
