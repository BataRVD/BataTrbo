using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrboPortal.TrboNet
{
    public class RequestMessage
    {
        public enum RequestType
        {
            Gps
        }
        public DeviceInformation Device { get; private set; }
        public RequestType Type { get; private set; }
        public DateTime TimeQueued { get; private set; }

        public RequestMessage(DeviceInformation device, RequestType requestType)
        {
            Device = device;
            TimeQueued = DateTime.Now;
            Type = requestType;
        }
    }
}
