namespace TrboPortal.Model.Api
{
    
    public partial class SystemSettings
    {
        [Newtonsoft.Json.JsonProperty("ServerInterval", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? ServerInterval { get; set; }

        [Newtonsoft.Json.JsonProperty("DefaultGpsMode", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public GpsModeEnum? DefaultGpsMode { get; set; }

        /// <example>test</example>
        /// <summary>hello</summary>
        [Newtonsoft.Json.JsonProperty("DefaultInterval", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? DefaultInterval { get; set; }

        [Newtonsoft.Json.JsonProperty("TurboNetSettings", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public TurboNetSettings TurboNetSettings { get; set; }

        [Newtonsoft.Json.JsonProperty("CiaBataSettings", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public CiaBataSettings CiaBataSettings { get; set; }


    }
}