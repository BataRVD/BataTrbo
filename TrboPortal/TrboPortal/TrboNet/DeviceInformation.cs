using NS.Enterprise.Objects.Devices;
using System;
using System.Collections.Concurrent;
using TrboPortal.Controllers;

namespace TrboPortal.TrboNet
{
    internal class DeviceInformation
    {
        public string deviceName { get; private set; }
        public GpsModeEnum gpsMode { get; private set; }
        int minimumServiceInterval;
        DateTime lastUpdate;
        ConcurrentStack<GpsMeasurement> gpsLocations;

        public DeviceInformation(int id)
        {
            // Enum.TryParse(Properties.Settings.Default.DefaultGpsMode, out GpsMode parsedGpsMode);
            gpsMode = GpsModeEnum.None;
            lastUpdate = DateTime.Now;
            deviceName = $"Radio {id}";
            // minimumServiceInterval = Properties.Settings.Default.DefaultGpsInterval;
            gpsLocations = new ConcurrentStack<GpsMeasurement>();
        }

        public void AddGpsLocation(GpsMeasurement gpsLocation)
        {
            gpsLocations.Push(gpsLocation);
        }

        public double TimeTillUpdate()
        {
            double secondsSinceUpdate = (DateTime.Now - lastUpdate).TotalSeconds;
            double secondsTillUpdate = minimumServiceInterval - secondsSinceUpdate;
            return secondsTillUpdate;
        }
    }
}