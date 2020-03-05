using TrboPortal.Mappers;
using TrboPortal.Model.Db;

namespace TrboPortal.Model.Api
{
    public class LogMessage
    {
        [Newtonsoft.Json.JsonProperty("DateTime", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string DateTime { get; set; }

        [Newtonsoft.Json.JsonProperty("Level", Required = Newtonsoft.Json.Required.Always)]
        public string Level { get; set; }

        [Newtonsoft.Json.JsonProperty("CallSite", Required = Newtonsoft.Json.Required.Default)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string CallSite { get; set; }

        [Newtonsoft.Json.JsonProperty("Message", Required = Newtonsoft.Json.Required.Default)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Message { get; set; }

        [Newtonsoft.Json.JsonProperty("Exception", Required = Newtonsoft.Json.Required.AllowNull)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Exception { get; set; }

        public LogMessage(LogEntry l)
        {
            DateTime = DateTimeMapper.ToString(l.Timestamp);
            Level = l.LogLevel;
            CallSite = l.CallSite ?? string.Empty;
            Message = l.Message ?? string.Empty;
            Exception = l.Exception;
        }
    }
}