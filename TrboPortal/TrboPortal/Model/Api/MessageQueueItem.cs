namespace TrboPortal.Model.Api
{
    
    public partial class MessageQueueItem
    {
        [Newtonsoft.Json.JsonProperty("Timestamp", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Timestamp { get; set; }

        [Newtonsoft.Json.JsonProperty("RadioID", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? RadioID { get; set; }


    }
}