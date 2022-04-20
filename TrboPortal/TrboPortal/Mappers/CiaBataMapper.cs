using TrboPortal.CiaBata;
using GpsMeasurement = TrboPortal.Model.Api.GpsMeasurement;

namespace TrboPortal.Mappers
{
    public static class CiaBataMapper
    {
        /// <summary>
        /// Maps API GpsMeasurement to CiaBata GPSLocation
        /// </summary>
        /// <param name="gpsMeasurement"></param>
        /// <returns></returns>
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
