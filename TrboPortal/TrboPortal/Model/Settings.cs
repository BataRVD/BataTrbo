using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrboPortal.Model
{
    public class Settings
    {
        public int ServerInterval { get; set; }

        public string DefaultGpsMode { get; set; }

        public int DefaultInterval { get; set; }

        public string TrboNetHost { get; set; }
        public int TrboNetPort { get; set; }
        public string TrboNetUser { get; set; }

        public string TrboNetPassword { get; set; }
    }
}