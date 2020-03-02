using System;

namespace TrboPortal.TrboNet
{
    public class RequestMessage
    {
        public enum RequestType
        {
            Gps
        }
        public int deviceID { get; private set; }
        public RequestType Type { get; private set; }
        public DateTime TimeQueued { get; private set; }

        public RequestMessage(int deviceID, RequestType requestType)
        {
            this.deviceID = deviceID;
            TimeQueued = DateTime.Now;
            Type = requestType;
        }
    }
}
