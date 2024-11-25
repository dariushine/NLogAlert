using NlogAlert;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton(new HttpClient());
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
