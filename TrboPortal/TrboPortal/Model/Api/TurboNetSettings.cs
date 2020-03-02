namespace TrboPortal.Model.Api
{
    
    public class TurboNetSettings
    {
        [Newtonsoft.Json.JsonProperty("Host", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Host { get; set; }

        [Newtonsoft.Json.JsonProperty("Port", Required = Newtonsoft.Json.Required.Always)]
        public int Port { get; set; }

        [Newtonsoft.Json.JsonProperty("User", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string User { get; set; }

        [Newtonsoft.Json.JsonProperty("Password", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Password { get; set; }


    }
}