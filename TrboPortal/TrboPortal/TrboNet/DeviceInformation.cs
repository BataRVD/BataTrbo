using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using TrboPortal.Controllers;
using Device = NS.Enterprise.Objects.Devices.Device;

namespace TrboPortal.TrboNet
{
    public class DeviceInformation
    {
        public int RadioID { get; }
        public int DeviceID { get; private set; }
        public string DeviceName { get; set; }
        public GpsModeEnum GpsMode { get; set; }
        public int MinimumServiceInterval { get; set; }
        public DateTime LastUpdate { get; set; }

        public ConcurrentStack<GpsMeasurement> GpsLocations { get; }
        public Device Device { get; set; }

        /// <summary>
        /// Constructor from TrboNet
        /// </summary>
        /// <param name="device"></param>
        public DeviceInformation(Device device)
        {
            this.RadioID = device.RadioID;
            this.DeviceID = device.ID;
            //TODO: What happens here? Default settings is constructed here, when are settings read?
            var settings = new SystemSettings();
            GpsMode = settings.DefaultGpsMode ?? GpsModeEnum.Interval;
            DeviceName = $"Radio {RadioID}";
            MinimumServiceInterval = settings.DefaultInterval ?? 60; // default interval in seconds
            GpsLocations = new ConcurrentStack<GpsMeasurement>();

            Device = device;

            //If created from Device assume it's for a measurement
            LastUpdate = device != null ? DateTime.Now : DateTime.UnixEpoch;
        }


        public void AddGpsLocation(GpsMeasurement gpsLocation)
        {
            GpsLocations.Push(gpsLocation);
        }

        public void UpdateDevice(Device device)
        {
            Device = device;
        }

        public double TimeTillUpdate()
        {
            var secondsSinceUpdate = (DateTime.Now - LastUpdate).TotalSeconds;

            var secondsTillUpdate = MinimumServiceInterval - secondsSinceUpdate;
            return secondsTillUpdate;
        }

        /// <summary>
        /// Updates configurable device settings (if defined) from new DeviceInformation
        /// TODO: Maybe model device options in separate object to shove into this thing
        /// </summary>
        /// <param name="di"></param>
        public void UpdateDeviceInfo(Controllers.Device di)
        {
            GpsMode = di.GpsMode ?? GpsMode;
            MinimumServiceInterval = di.RequestInterval ?? MinimumServiceInterval;
        }
    }
}