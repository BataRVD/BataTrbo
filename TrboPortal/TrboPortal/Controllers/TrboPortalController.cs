using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TrboPortal.Mappers;
using TrboPortal.Model.Api;
using TrboPortal.TrboNet;
using GpsMeasurement = TrboPortal.Model.Api.GpsMeasurement;

namespace TrboPortal.Controllers
{
    [RoutePrefix("TrboPortal/v1")]
    public class TrboPortalController : ApiController
    {
        /// <summary>List of all radios</summary>
        /// <param name="radioIds">Tags to filter by</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("radio")]
        public Task<ICollection<Model.Api.Radio>> GetRadios([FromUri] IEnumerable<int> radioIds)
        {
            return Task.FromResult<ICollection<Model.Api.Radio>>(TurboController.Instance.GetRadioSettings(radioIds));
        }

        /// <summary>Update gps mode and interval radios</summary>
        /// <param name="radioSettings">Radios to update</param>
        [HttpPatch, Route("radio")]
        public Task UpdateRadioSettings([FromBody] [Required] IEnumerable<Radio> radioSettings)
        {
            if (radioSettings == null || !radioSettings.Any())
            {
                var message = "RadioSettings need to be supplied";
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }

            return TrboPortalHelper.UpdateRadioSettingsAsync(radioSettings);
        }

        /// <summary>Returns last known GPS position of radio(s)</summary>
        /// <param name="ids">Radios</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("gps")]
        public Task<ICollection<GpsMeasurement>> GetMostRecentGps([FromUri] IEnumerable<int> ids)
        {
            return TrboPortalHelper.GetGpsMeasurementsAsync(ids, null, null);
        }

        /// <summary>Returns last known GPS position of radio(s)</summary>
        /// <param name="id">Radios</param>
        /// <param name="from">From TimeStamp for GPS measurements to get</param>
        /// <param name="through">Through TimeStamp for GPS measurements to get</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("gps/history")]
        public Task<ICollection<GpsMeasurement>> GetGpsHistory([FromUri] IEnumerable<int> id, [FromUri] string from, [FromUri] string through)
        {
            var f = DateTimeMapper.ToDateTime(from);
            var t = DateTimeMapper.ToDateTime(through);
            if (f.HasValue && t.HasValue && f > t)
            {
                var message = "From need to be before through";
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }


            return TrboPortalHelper.GetGpsMeasurementsAsync(id, f, t);
        }

        /// <summary>Request GPS opdate for radio(s)</summary>
        /// <param name="id">Radios</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("gps/update")]
        public Task RequestGpsUpdate([FromUri] IEnumerable<int> radioIds)
        {
            if (radioIds == null || radioIds.Count() == 0)
            {
                var message = "RadioIds are required";
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }

            return Task.Run(() => TurboController.Instance.PollForGps(radioIds));
        }

        /// <summary>TrboNet message queue</summary>
        /// <returns>Messages in TrboNet queue</returns>
        [HttpGet, Route("system/queue")]
        public Task<ICollection<MessageQueueItem>> GetMessageQueue()
        {
            return Task.FromResult<ICollection<MessageQueueItem>>(TurboController.Instance.GetRequestQueue()
                .Select(rqi => MessageQueueMapper.Map(rqi))
                .OrderBy(i => i.Timestamp)
                .ToList()
            );
        }

        /// <summary>List of all radios</summary>
        /// <param name="id">Tags to filter by</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("system/clearQueue")]
        public Task ClearMessageQueue([FromUri] IEnumerable<int> id)
        {
            return Task.Run(() => TurboController.Instance.ClearRequestQueue(id));
        }


        /// <summary>System settings</summary>
        /// <returns>Blaat</returns>
        [HttpGet, Route("system/settings")]
        public Task<SystemSettings> GetSystemSettings()
        {
            return TrboPortalHelper.GetSystemSettings();
        }

        /// <summary>Update System settings</summary>
        /// <param name="settings">System settings to store</param>
        [HttpPatch, Route("system/settings")]
        public Task SetSystemSettings([FromBody] [Required] SystemSettings settings)
        {
            if (settings == null)
            {
                var message = "Settings need to be supplied";
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }

            return TrboPortalHelper.UpdateSystemSettingsAsync(settings);
        }

        /// <summary>System logging</summary>
        /// <param name="loglevel">Log level filter of logging to return</param>
        /// <param name="from">DateTime From filter of logging to return</param>
        /// <param name="through">DateTime Through filter of logging to return</param>
        /// <returns>Blaat</returns>
        [HttpGet, Route("system/logs")]
        public Task<ICollection<LogMessage>> GetLogging([Required] string loglevel, [FromUri] string from = null, [FromUri] string through = null)
        {
            if (string.IsNullOrEmpty(loglevel))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Should supply loglevel value"));
            }

            return TrboPortalHelper.GetLoggingAsync(loglevel, from, through);
        }

        /// <summary>Returns the current working directory</summary>
        [HttpGet, Route("system/working_directory")]
        public string GetWorkingDirectory() => Directory.GetCurrentDirectory();
    }
}