using Newtonsoft.Json;
using NLog;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TrboPortal.Model.Api;

namespace TrboPortal.CiaBata
{
    public class CiaBata
    {
        public int edition { get; set; } 
        public string url { get; set; }

        public string token
        {
            set => httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static HttpClient httpClient = new HttpClient();


        public CiaBata(string url, string token, int edition)
        {
            this.url = url;
            this.token = token;
            this.edition = edition;
        }

        /// <summary>
        /// Post the current location to ciabata
        /// </summary>
        /// <param name="gps"></param>
        public async Task PostGpsLocation(GpsMeasurement measurement)
        {
            if (string.IsNullOrEmpty(url))
            {
                logger.Warn("Ciabata is not configured, won't send data");
                return;
            }
            try
            {
                var gps = new GPSLocation
                {
                    edition = $"api/editions/{edition}",
                    latitude = measurement.Latitude,
                    longitude = measurement.Longitude,
                    externalId = measurement.RadioID,
                    rssid = (float)(measurement.Rssi ?? 0)
                };

                string json = JsonConvert.SerializeObject(gps, Formatting.None);
                logger.Info("PostGpsLocation " + json);
                
                var content = new StringContent(json, Encoding.UTF8, "application/ld+json");
                var result = await httpClient.PostAsync($"{url}/logistics/positions", content);

                logger.Debug("PostGpsLocation result:" + result.StatusCode);

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    logger.Info("Error with PostGpsLocation " + json);
                }
                
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Problem sending location to CiaBata");
            }
        }

        /// <summary>
        /// Post a livesign to the server, for example for a radio or the server
        /// </summary>
        /// <param name="referenceID"></param>
        /// <param name="name"></param>
        /// <param name="online"></param>
        /// <param name="lastSeen"></param>
        public async void PutDeviceLifeSign(int referenceID, string name, bool online, DateTime lastSeen)
        {
            if (string.IsNullOrEmpty(url))
            {
                logger.Warn("Ciabata is not configured, won't send data");
                return;
            }
            try
            {
                DeviceLifeSign gps = new DeviceLifeSign
                {
                    edition = $"api/editions/{edition}",
                    name = name,
                    externalID = referenceID,
                    isActive = online,
                    timestampActive = lastSeen.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                var request = JsonConvert.SerializeObject(gps, Formatting.None);
                logger.Info("PutDeviceLifeSign " + request);

                var content = new StringContent(request, Encoding.UTF8, "application/ld+json");
                var response = await httpClient.PutAsync($"{url}/logistics/trackers", content);

                logger.Debug("PutDeviceLifeSign result :" + response.StatusCode);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    logger.Error($"Error with PutDeviceLifeSign for device '{gps.name}' ({response.StatusCode})!" +
                        $"{Environment.NewLine}Request: '{request}'. {Environment.NewLine}Response: '{response}'.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Could not post livesign for '{name}' with referenceID {referenceID}");
            }
        }

    }
}
