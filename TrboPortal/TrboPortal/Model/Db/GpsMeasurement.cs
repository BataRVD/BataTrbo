using TrboPortal.Mappers;
using TrboPortal.Model.Db;

namespace TrboPortal.Model.Api
{
    //TODO This shouldn't be a partial class of Model.Api
    public partial class GpsMeasurement
    {
        public GpsMeasurement()
        {
            //Default constructor
        }

        public GpsMeasurement(GpsEntry g)
        {
            //TODO JV
            RadioID = g.RadioId ?? 0;
            Timestamp = DateTimeMapper.ToString(g.Timestamp);
            Latitude = g.Latitude;
            Longitude = g.Longitude;
            Rssi = g.Rssi;
        }
        
    }
}