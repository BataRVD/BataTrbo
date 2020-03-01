using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrboPortal.CiaBata;
using TrboPortal.Controllers;

namespace TrboPortal.Mappers
{
    public static class CiaBataMapper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static GPSLocation ToGpsLocation(GpsMeasurement gpsMeasurement)
        {
            return new GPSLocation
            {
                deviceName = $"Radio {gpsMeasurement.RadioID}", // TODO, okay for ciabata?
                Latitude = gpsMeasurement.Latitude,
                Longitude = gpsMeasurement.Longitude,
                RadioID = gpsMeasurement.RadioID,
                Rssi = (float)(gpsMeasurement.Rssi ?? 0)
            };
        }

    }
}
