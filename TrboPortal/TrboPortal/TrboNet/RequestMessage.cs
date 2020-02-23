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
        public int DeviceId { get; private set; }
        public RequestType Type { get; private set; } 
        public DateTime TimeQueued { get; private set; }

        public RequestMessage(int deviceId)
        {
            DeviceId = deviceId;
            TimeQueued = DateTime.Now;
            Type = RequestType.Gps;
        }
    }
}
