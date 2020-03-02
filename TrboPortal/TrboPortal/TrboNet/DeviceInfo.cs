using System;
using System.Collections.Concurrent;
using Device = NS.Enterprise.Objects.Devices.Device;
using GpsMeasurement = TrboPortal.Model.Api.GpsMeasurement;

namespace TrboPortal.TrboNet
{
    public class DeviceInfo
    {
        /*Dingen die we hier willen hebben
         *
         * gpsinfo[]
         * Device
         */

        public int RadioID { get; private set; }
        public DateTime LastUpdate { get; set; }
        public ConcurrentStack<GpsMeasurement> GpsLocations { get; }
        public Device Device { get; set; }
        public int DeviceID { get; internal set; }

        public DeviceInfo(Device device)
        {
            this.Device = device;
            this.DeviceID = device.ID;
            this.RadioID = device.RadioID;
            GpsLocations = new ConcurrentStack<GpsMeasurement>();
            LastUpdate = device != null ? DateTime.Now : DateTime.MinValue;
        }

        public DeviceInfo(int deviceID)
        {
            DeviceID = deviceID;
            RadioID = -1; //???
            GpsLocations = new ConcurrentStack<GpsMeasurement>();
            LastUpdate = DateTime.Now;
        }

        internal void UpdateDevice(Device device)
        {
            Device = device;
            this.RadioID = device.RadioID;

        }

    }
}