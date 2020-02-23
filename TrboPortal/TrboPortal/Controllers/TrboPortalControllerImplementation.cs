using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrboPortal.Controllers
{
    public class TrboPortalControllerImplementation : ITrboPortalController
    {
        public Task<ICollection<Device>> GetDevicesAsync(IEnumerable<int> id)
        {
            var bert = new List<Device>();
            bert.Add(new Device
                {GpsModeEnum = GpsModeEnum.Interval, Id = 9001, Name = "BlaatDevice", RequestPrio = RequestPrio.High});
            return Task.FromResult<ICollection<Device>>(bert);
        }

        public Task<ICollection<GpsMeasurement>> GetGpsHistoryAsync(IEnumerable<int> id, string from, string through)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<string>> GetLoggingAsync(string loglevel, string from, string through)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<MessageQueueItem>> GetMessageQueueAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<GpsMeasurement>> GetMostRecentGpsAsync(IEnumerable<int> id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<SystemSettings>> GetSystemSettingsAsync(SystemSettings body)
        {
            throw new NotImplementedException();
        }

        public Task RequestGpsUpdateAsync(IEnumerable<int> id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<SystemSettings>> SetSystemSettingsAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateGpsModeAsync(IEnumerable<int> id, GpsMode body)
        {
            throw new NotImplementedException();
        }
    }
}