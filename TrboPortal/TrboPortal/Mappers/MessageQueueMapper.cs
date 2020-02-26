using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TrboPortal.Controllers;
using TrboPortal.TrboNet;

namespace TrboPortal.Mappers
{
    public class MessageQueueMapper
    {
        public static MessageQueueItem Map(DeviceInformation di, RequestMessage rm)
        {
            return new MessageQueueItem
            {
                Device = DeviceMapper.MapToDevice(di),
                Timestamp = rm.TimeQueued.ToString("o")
            };
        }
    }
}