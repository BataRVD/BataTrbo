using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using TrboPortal.Mappers;
using TrboPortal.Model;
using Exception = System.Exception;

namespace TrboPortal.Controllers
{
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