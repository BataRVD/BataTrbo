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
        public DateTime LastUpdateRequest { get; set; }

        public DateTime LastMessageReceived { get; set; }

        public ConcurrentStack<GpsMeasurement> GpsLocations { get; }
        public Device Device { get; set; }
        public int DeviceID { get; internal set; }

        public DeviceInfo(Device device)
        {
            Device = device;
            DeviceID = device.ID;
            RadioID = device.RadioID;
            GpsLocations = new ConcurrentStack<GpsMeasurement>();
            LastUpdateRequest = device != null ? DateTime.Now : DateTime.MinValue;
            LastMessageReceived = device != null ? DateTime.Now : DateTime.MinValue;
        }

        public DeviceInfo(int deviceID)
        {
            DeviceID = deviceID;
            RadioID = -1; //???
            GpsLocations = new ConcurrentStack<GpsMeasurement>();
            LastUpdateRequest = DateTime.Now;
            LastMessageReceived = DateTime.Now;
        }

        internal void UpdateDevice(Device device)
        {
            Device = device;
            RadioID = device.RadioID;
        }

    }
}