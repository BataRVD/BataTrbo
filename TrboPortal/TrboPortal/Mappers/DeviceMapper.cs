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
        public static Device MapToDevice(DeviceInformation di)
        {
            return new Device
            {
                GpsMode = di.gpsMode,
                Id = di.device.ID,
                Name = di.deviceName,
                RequestInterval = di.minimumServiceInterval

            };
        }
    }
}