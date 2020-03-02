namespace TrboPortal.Model.Api
{
    
    public class CiaBataSettings
    {
        [Newtonsoft.Json.JsonProperty("Host", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Host { get; set; }


    }
}