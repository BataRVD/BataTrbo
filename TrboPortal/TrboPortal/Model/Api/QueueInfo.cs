using System.Collections.Generic;

namespace TrboPortal.Model.Api
{

    public partial class QueueInfo
    {
        // Total number of location request messages sent to MotoTrboServer since startup
        [Newtonsoft.Json.JsonProperty("LocationRequestCounter", Required = Newtonsoft.Json.Required.Always)]
        long LocationRequestCounter;
        
        // Total number of location response messages received from MotoTrboServer since startup
        [Newtonsoft.Json.JsonProperty("LocationResponseCounter", Required = Newtonsoft.Json.Required.Always)]
        long LocationResponseCounter;

        // Total number of location response messages received from MotoTrboServer since startup
        [Newtonsoft.Json.JsonProperty("InternalTrboNetQueueSize", Required = Newtonsoft.Json.Required.Always)]
        long InternalTrboNetQueueSize;

        [Newtonsoft.Json.JsonProperty("MessageQueueItems", Required = Newtonsoft.Json.Required.Always)]
        ICollection<MessageQueueItem> MessageQueueItems;

        public QueueInfo() { }
        public QueueInfo(List<MessageQueueItem> messageQueueItems, long locationRequestCounter, long locationResponseCounter, long internalTrboNetQueueSize)
        {
            MessageQueueItems = messageQueueItems;
            LocationRequestCounter = locationRequestCounter;
            LocationResponseCounter = locationResponseCounter;
            InternalTrboNetQueueSize = internalTrboNetQueueSize;
        }
    }

    public partial class MessageQueueItem
    {
        [Newtonsoft.Json.JsonProperty("Timestamp", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Timestamp { get; set; }

        [Newtonsoft.Json.JsonProperty("RadioID", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? RadioID { get; set; }


    }
}