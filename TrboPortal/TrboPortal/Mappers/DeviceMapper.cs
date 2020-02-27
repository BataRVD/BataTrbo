using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrboPortal.Controllers;
using TrboPortal.TrboNet;

namespace TrboPortal.Mappers
{
    public class DeviceMapper
    {
        /// <summary>
        /// Maps DeviceInformation to API Device
        /// </summary>
        /// <param name="di"></param>
        /// <returns></returns>
        public static Device MapToDevice(DeviceInformation di)
        {
            return new Device
            {
                GpsMode = di.GpsMode,
                Id = di.Device.ID,
                Name = di.DeviceName,
                RequestInterval = di.MinimumServiceInterval
            };
        }


        /// <summary>
        /// Maps API device to DeviceInformation.
        /// </summary>
        /// <param name="d">API Device</param>
        /// <returns></returns>
        public static DeviceInformation MapToDeviceInformation(Device d)
        {
            var di = new DeviceInformation(d.Id, null)
            {
                DeviceName = d.Name,
            };

            di.GpsMode = d.GpsMode ?? di.GpsMode;
            di.MinimumServiceInterval = d.RequestInterval ?? di.MinimumServiceInterval;

            return di;
        }

    }
}