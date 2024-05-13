using System.ComponentModel.DataAnnotations;

namespace TrboPortal.Model.Api
{

    public class CiaBataSettings
    {
        [Newtonsoft.Json.JsonProperty("Edition", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [Range(1, 100)]
        public int Edition { get; set; }

        [Newtonsoft.Json.JsonProperty("Host", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [StringLength(99, MinimumLength = 2)]
        public string Host { get; set; }
        [Newtonsoft.Json.JsonProperty("Token", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [StringLength(99, MinimumLength = 2)]
        public string Token { get; set; }
    }
}