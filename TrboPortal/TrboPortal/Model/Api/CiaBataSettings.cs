using System.ComponentModel.DataAnnotations;

namespace TrboPortal.Model.Api
{

    public class CiaBataSettings
    {
        [Newtonsoft.Json.JsonProperty("Host", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [StringLength(99, MinimumLength = 2)]
        public string Host { get; set; }
    }
}