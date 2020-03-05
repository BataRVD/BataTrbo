using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TrboPortal.Mappers;

namespace TrboPortal.Model.Db
{
    /// <summary>
    /// Model used for database storage, when changing please look into the default migration options of entityframework
    ///</summary>
    public class LogEntry
    {
        [Key]
        public int LogEntryId { set; get; }

        public long Timestamp { get; set; }
        public string LogLevel { get; set; }
        public string CallSite { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }

        public override string ToString() => $"{DateTimeMapper.ToString(Timestamp)} {LogLevel} {CallSite} {Message} {Exception}";
    }
}