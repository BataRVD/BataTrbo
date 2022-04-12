﻿using System;

namespace TrboPortal.TrboNet
{
    public class RequestMessage
    {
        public enum RequestType
        {
            Gps
        }
        public int deviceID { get; private set; }
        public int radioID { get; private set; }
        public RequestType Type { get; private set; }
        public DateTime TimeQueued { get; private set; }

        public RequestMessage(DeviceInfo device, RequestType requestType) : this(device.DeviceID, device.RadioID, requestType)
        {
        }

        public RequestMessage(int deviceId, int radioId, RequestType requestType)
        {
            this.deviceID = deviceId;
            this.radioID = radioId;
            TimeQueued = DateTime.Now;
            Type = requestType;
        }

        public override string ToString()
        {
            return $"[{deviceID}] {Type.ToString()} - {TimeQueued.ToString()}";
        }
    }
}
