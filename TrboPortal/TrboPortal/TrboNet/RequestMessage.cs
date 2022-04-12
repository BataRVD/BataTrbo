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

        public override string ToString()
        {
            return $"[{deviceID}] {Type.ToString()} - {TimeQueued.ToString()}";
        }

        public override bool Equals(object obj)
        {
            //return (obj as RequestMessage)?.deviceID == this.deviceID;
            if (obj != null)
            {
                if (obj is RequestMessage otherDevice)
                {
                    return otherDevice.deviceID == this.deviceID;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.deviceID;
        }
    }
}
