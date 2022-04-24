using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using TrboPortal.Mappers;
using TrboPortal.Model.Api;
using TrboPortal.TrboNet;
using GpsMeasurement = TrboPortal.Model.Api.GpsMeasurement;

namespace TrboPortal.Controllers
{
    [RoutePrefix("api/v1")]
    public class ApiController : System.Web.Http.ApiController
    {
        /// <summary>List of all radios</summary>
        /// <param name="radioIds">Tags to filter by</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("radio")]
        public Task<IEnumerable<Model.Api.Radio>> GetRadios([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] radioIds = null)
        {
            return ApiHelper.GetRadioSettingsAsync(radioIds);
        }

        [HttpDelete, Route("radio")]
        public Task DeleteRadios([ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] radioIds = null)
        {
            return ApiHelper.DeleteRadioSettings(radioIds);
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

            return ApiHelper.UpdateRadioSettingsAsync(radioSettings);
        }

        /// <summary>Returns last known GPS position of radio(s)</summary>
        /// <param name="ids">Radios</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("gps")]
        public Task<List<GpsMeasurement>> GetMostRecentGps([ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[]  ids = null)
        {
            return ApiHelper.GetGpsMeasurementsAsync(ids, null, null);
        }

        /// <summary>Returns last known GPS position of radio(s)</summary>
        /// <param name="id">Radios</param>
        /// <param name="from">From TimeStamp for GPS measurements to get</param>
        /// <param name="through">Through TimeStamp for GPS measurements to get</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("gps/history")]
        public Task<List<GpsMeasurement>> GetGpsHistory([ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[]  id = null, [FromUri] string from = null, [FromUri] string through = null)
        {
            var f = DateTimeMapper.ToDateTime(from);
            var t = DateTimeMapper.ToDateTime(through);
            if (f.HasValue && t.HasValue && f > t)
            {
                var message = "From need to be before through";
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }


            return ApiHelper.GetGpsMeasurementsAsync(id, f, t);
        }

        /// <summary>Request GPS opdate for radio(s)</summary>
        /// <param name="id">Radios</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("gps/update")]
        public HttpResponseMessage RequestGpsUpdate([ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[]  radioIds) 
        {
            var errors = ApiHelper.RequestGpsUpdate(radioIds);
            if (errors.Any())
            {
                List<string> messages = new List<string> { "Failed to request one or more GPS updates!" };
                messages.AddRange(errors.Select(e => e.ToString()).ToList());
                var errorResponse = Request.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.ReasonPhrase = "Failed to request one or more GPS updates!";
                errorResponse.Content = new StringContent(JsonConvert.SerializeObject(messages), Encoding.UTF8, "application/json");
                return errorResponse;
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>TrboNet message queue</summary>
        /// <returns>Messages in TrboNet queue</returns>
        [HttpGet, Route("system/queue")]
        public Task<QueueInfo> GetMessageQueue()
        {
            return Task.FromResult<QueueInfo>(ApiHelper.GetQueueInfo());
        }

        /// <summary>List of all radios</summary>
        /// <param name="id">Tags to filter by</param>
        /// <returns>successful operation</returns>
        [HttpGet, Route("system/clearQueue")]
        public Task ClearMessageQueue([ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[]  id = null)
        {
            return Task.Run(() => TurboController.Instance.ClearRequestQueue(id));
        }


        /// <summary>System settings</summary>
        /// <returns>Blaat</returns>
        [HttpGet, Route("system/settings")]
        public Task<SystemSettings> GetSystemSettings()
        {
            return ApiHelper.GetSystemSettings();
        }

        /// <summary>Update System settings</summary>
        /// <param name="settings">System settings to store</param>
        [HttpPatch, Route("system/settings")]
        public Task<HttpResponseMessage> SetSystemSettings([FromBody] [Required] SystemSettings settings)
        {
            return Task.Run(() =>
            {
                if (ModelState.IsValid)
                {
                    // Do something with the product (not shown).
                    _ = ApiHelper.UpdateSystemSettingsAsync(settings);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            });
        }

        /// <summary>System logging</summary>
        /// <param name="loglevel">Log level filter of logging to return</param>
        /// <param name="from">DateTime From filter of logging to return</param>
        /// <param name="through">DateTime Through filter of logging to return</param>
        /// <returns>Blaat</returns>
        [HttpGet, Route("system/logs")]
        public Task<ICollection<LogMessage>> GetLogging([Required] string loglevel = "Info", [FromUri] string from = null, [FromUri] string through = null)
        {
            if (string.IsNullOrEmpty(loglevel))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Should supply loglevel value"));
            }

            return ApiHelper.GetLoggingAsync(loglevel, from, through);
        }
    }
}