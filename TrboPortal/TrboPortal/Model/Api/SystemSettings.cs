using System.ComponentModel.DataAnnotations;

namespace TrboPortal.Model.Api
{

    public partial class SystemSettings
    {
        [Newtonsoft.Json.JsonProperty("ServerInterval", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Range(1, 3600000)]
        public int? ServerInterval { get; set; }

        [Newtonsoft.Json.JsonProperty("DefaultGpsMode", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public GpsModeEnum? DefaultGpsMode { get; set; }

        /// <example>test</example>
        /// <summary>hello</summary>
        [Newtonsoft.Json.JsonProperty("DefaultInterval", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Range(1, 3600)]
        public int? DefaultInterval { get; set; }

        [Newtonsoft.Json.JsonProperty("TurboNetSettings", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Required]
        public TurboNetSettings TurboNetSettings { get; set; }

        [Newtonsoft.Json.JsonProperty("CiaBataSettings", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Required]
        public CiaBataSettings CiaBataSettings { get; set; }

    }
}