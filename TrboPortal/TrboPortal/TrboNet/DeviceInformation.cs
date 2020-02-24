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
        ConcurrentStack<GpsMeasurement> gpsLocations;
        public Device device { get; private set; }

        public DeviceInformation(int id, Device device)
        {
            SystemSettings settings = new SystemSettings();
            
            gpsMode = settings.DefaultGpsMode?? GpsModeEnum.Interval;  
            lastUpdate = DateTime.Now;
            deviceName = $"Radio {id}";
            minimumServiceInterval = settings.DefaultInterval ?? 60; // default interval in seconds
            gpsLocations = new ConcurrentStack<GpsMeasurement>();
            this.device = device;
        }

        public void AddGpsLocation(GpsMeasurement gpsLocation)
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