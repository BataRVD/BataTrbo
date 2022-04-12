using System.ComponentModel.DataAnnotations;

namespace TrboPortal.Model.Api
{

    public class TurboNetSettings
    {
        [Newtonsoft.Json.JsonProperty("Host", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [StringLength(99, MinimumLength = 2)]
        public string Host { get; set; }

        [Newtonsoft.Json.JsonProperty("Port", Required = Newtonsoft.Json.Required.Always)]
        [Range(1, 64000)]
        public int Port { get; set; }

        [Newtonsoft.Json.JsonProperty("User", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [StringLength(99, MinimumLength = 2)]
        public string User { get; set; }

        [Newtonsoft.Json.JsonProperty("Password", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [StringLength(99, MinimumLength = 2)]
        public string Password { get; set; }
    }
}