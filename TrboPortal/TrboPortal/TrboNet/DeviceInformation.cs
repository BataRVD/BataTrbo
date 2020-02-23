using System;
using System.Collections.Concurrent;
using TrboPortal.Controllers;
using Device = NS.Enterprise.Objects.Devices.Device;

namespace TrboPortal.TrboNet
{
    public class DeviceInformation
    {
        public string deviceName { get; private set; }
        public GpsModeEnum gpsMode { get; private set; }
        public int minimumServiceInterval { get; private set; }
        DateTime lastUpdate;
        ConcurrentStack<GPSLocation> gpsLocations;
        public Device device { get; private set; }

        public DeviceInformation(int id, Device device)
        {
            /*
            Enum.TryParse(Properties.Settings.Default.DefaultGpsMode, out GpsMode parsedGpsMode);
            gpsMode = parsedGpsMode;
            */
            gpsMode = GpsModeEnum.None;
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