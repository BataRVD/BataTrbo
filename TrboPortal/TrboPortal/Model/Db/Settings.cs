using System.ComponentModel.DataAnnotations;

namespace TrboPortal.Model.Db
{
    public class Settings
    {
        [Key] public int SettingsId { get; set; }
        public int ServerInterval { get; set; }

        public string DefaultGpsMode { get; set; }

        public int DefaultInterval { get; set; }

        public string TrboNetHost { get; set; }
        public int TrboNetPort { get; set; }
        public string TrboNetUser { get; set; }

        public string TrboNetPassword { get; set; }
        public string CiaBataHost { get; set; }

    }
}