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
        private string user;
        private string password;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static HttpClient httpClient = new HttpClient();


        public CiaBata(string url, string user, string password)
        {
            this.url = url;
            this.user = user;
            this.password = password;
        }

        public async void PostGpsLocation(GPSLocation gps)
        {
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


        public async void PostDeviceLifeSign(int RadioID, string deviceName, bool online)
        {
            try
            {
                DeviceLifeSign gps = new DeviceLifeSign();
                gps.deviceName = deviceName;
                gps.RadioID = RadioID;

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
                logger.Error(ex, $"Could not post device livesign for radioID {RadioID}");
            }
        }

    }
}
