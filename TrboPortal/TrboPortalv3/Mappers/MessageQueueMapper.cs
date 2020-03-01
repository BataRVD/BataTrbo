﻿using System;
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
        public static MessageQueueItem Map(RequestMessage rm)
        {
            return new MessageQueueItem
            {
                RadioID = 666, // TODO "maak er wat van"
                Timestamp = DateTimeMapper.ToString(rm.TimeQueued)
            };
        }
    }
}