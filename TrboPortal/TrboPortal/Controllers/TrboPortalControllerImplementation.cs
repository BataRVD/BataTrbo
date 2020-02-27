using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrboPortal.Mappers;
using TrboPortal.TrboNet;

namespace TrboPortal.Controllers
{
    public class TrboPortalControllerImplementation : ITrboPortalController
    {
        public Task<ICollection<Radio>> GetRadiosAsync(IEnumerable<int> id)
        {
            return Task.FromResult<ICollection<Radio>>(TurboController.Instance.GetSettings());
        }

        public Task UpdateRadioSettingsAsync(IEnumerable<Radio> devices)
        {
            return Task.Run(() => devices?.ToList().ForEach(d => { TurboController.Instance.AddOrUpdateDeviceSettings(d); }));
        }

        public Task<ICollection<GpsMeasurement>> GetGpsHistoryAsync(IEnumerable<int> id, string from, string through)
        {
            var f = DateTimeMapper.ToDateTime(from);
            var t = DateTimeMapper.ToDateTime(through);

            //TODO What to do if from/through wasn't convertible to DateTime? Log, Return error code or continue?
            return Task.FromResult(TrboPortalHelper.GetGpsMeasurements(id, f, t));
        }

        public Task<ICollection<string>> GetLoggingAsync(string loglevel, string from, string through)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<MessageQueueItem>> GetMessageQueueAsync()
        {
            return Task.FromResult<ICollection<MessageQueueItem>>(TurboController.Instance.GetRequestQueue()
                .Select(rqi => MessageQueueMapper.Map(rqi))
                .OrderBy(i => i.Timestamp)
                .ToList()
            );
        }

        public Task<ICollection<GpsMeasurement>> GetMostRecentGpsAsync(IEnumerable<int> id)
        {
            return Task.FromResult(TrboPortalHelper.GetGpsMeasurements(id, null, null));
        }

        public Task<ICollection<SystemSettings>> GetSystemSettingsAsync(SystemSettings body)
        {
            throw new NotImplementedException();
        }

        public Task RequestGpsUpdateAsync(IEnumerable<int> id)
        {
            // TODO never null ?
            return Task.Run(() => TurboController.Instance.PollForGps(id.ToArray()));
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