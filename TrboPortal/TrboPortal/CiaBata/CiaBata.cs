using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TrboPortal.CiaBata
{
    public class CiaBata
    {
        private string url;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static HttpClient httpClient = new HttpClient();


        public CiaBata(string url)
        {
            this.url = url;
        }

        /// <summary>
        /// Post the current location to ciabata
        /// </summary>
        /// <param name="gps"></param>
        public async void PostGpsLocation(GPSLocation gps)
        {
            if (string.IsNullOrEmpty(url))
            {
                logger.Warn("Ciabata is not configured, won't send data");
                return;
            }
            try
            {
                string json = JsonConvert.SerializeObject(gps, Formatting.None);
                logger.Info("PostGpsLocation " + json);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(url, content);

                logger.Info("PostObject :" + result.StatusCode);

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
        public async void PostDeviceLifeSign(int referenceID, string name, bool online)
        {
            if (string.IsNullOrEmpty(url))
            {
                logger.Warn("Ciabata is not configured, won't send data");
                return;
            }
            try
            {
                DeviceLifeSign gps = new DeviceLifeSign();
                gps.deviceName = name;
                gps.RadioID = referenceID;

                if (online)
                {
                    gps.status = "online";
                }
                else
                {
                    gps.status = "offline";
                }

                string json = JsonConvert.SerializeObject(gps, Formatting.None);
                logger.Info("PostDeviceLifeSign " + json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(url, content);

                logger.Info("PostObject :" + result.StatusCode);
                
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    logger.Info("Error with PostDeviceLifeSign " + json);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Could not post livesign for '{name}' with referenceID {referenceID}");
            }
        }

    }
}
