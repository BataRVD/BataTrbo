using TrboPortal.Model.Api;
using TrboPortal.TrboNet;

namespace TrboPortal.Mappers
{
    public class MessageQueueMapper
    {
        public static MessageQueueItem Map(RequestMessage rm)
        {
            return new MessageQueueItem
            {
                RadioID = rm.radioID,
                Timestamp = DateTimeMapper.ToString(rm.TimeQueued)
            };
        }
    }
}