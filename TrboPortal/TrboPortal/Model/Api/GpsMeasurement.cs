using TrboPortal.Mappers;
using TrboPortal.Model.Db;

namespace TrboPortal.Model.Api
{
    
    public partial class GpsMeasurement
    {
        [Newtonsoft.Json.JsonProperty("RadioID", Required = Newtonsoft.Json.Required.Always)]
        public int RadioID { get; set; }

        [Newtonsoft.Json.JsonProperty("DeviceID", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? DeviceID { get; set; }

        [Newtonsoft.Json.JsonProperty("Timestamp", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Timestamp { get; set; }

        [Newtonsoft.Json.JsonProperty("Latitude", Required = Newtonsoft.Json.Required.Always)]
        public double Latitude { get; set; }

        [Newtonsoft.Json.JsonProperty("Longitude", Required = Newtonsoft.Json.Required.Always)]
        public double Longitude { get; set; }

        [Newtonsoft.Json.JsonProperty("Rssi", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double? Rssi { get; set; }

        public GpsMeasurement() { }
        public GpsMeasurement(GpsEntry g)
        {
            RadioID = g.RadioId ?? -1;
            Timestamp = DateTimeMapper.ToString(g.Timestamp);
            Latitude = g.Latitude;
            Longitude = g.Longitude;
            Rssi = g.Rssi;
        }
    }
}