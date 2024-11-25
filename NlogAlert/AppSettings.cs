using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NlogAlert
{
    public class AppSettings
    {
        public required string LogDirectory { get; set; }
        public required string LogFileName { get; set; }
        public required string SearchPattern { get; set; }
        public required string ApiUrl { get; set; }
        public required string Type { get; set; }
        public required string Content { get; set; }
        public required int Delay { get; set; }

    }
}