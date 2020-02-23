using NS.Enterprise.Objects.Devices;
using System;
using System.Collections.Concurrent;

namespace TrboPortal.TrboNet
{
    enum GpsMode
    {
        none = 0, // default value, will be used as fallback on the TryParse
        pull = 1,
        interval = 2
    }

    internal class DeviceInformation
    {
        public string deviceName { get; private set; }
        public GpsMode gpsMode { get; private set; }
        int minimumServiceInterval;
        DateTime lastUpdate;
        ConcurrentStack<GPSLocation> gpsLocations;
        public Device device { get; private set; }

        public DeviceInformation(int id, Device device)
        {
            /*
            Enum.TryParse(Properties.Settings.Default.DefaultGpsMode, out GpsMode parsedGpsMode);
            gpsMode = parsedGpsMode;
            */
            gpsMode = GpsMode.none;
            lastUpdate = DateTime.Now;
            deviceName = $"Radio {id}";
            minimumServiceInterval = 10; //Properties.Settings.Default.DefaultGpsInterval;
            gpsLocations = new ConcurrentStack<GPSLocation>();
            this.device = device;
        }

        public void AddGpsLocation(GPSLocation gpsLocation)
        {
            gpsLocations.Push(gpsLocation);
        }

        public void UpdateDevice(Device device)
        {
            this.device = device;
        }

        public double TimeTillUpdate()
        {
            double secondsSinceUpdate = (DateTime.Now - lastUpdate).TotalSeconds;
            double secondsTillUpdate = minimumServiceInterval - secondsSinceUpdate;
            return secondsTillUpdate;
        }
    }
}