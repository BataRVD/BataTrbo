using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrboPortal.Model;
using TrboPortal.Controllers;
using NLog;

namespace TrboPortal.Mappers
{
    public static class DatabaseMapper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Model.Radio Map(Controllers.Radio radio)
        {
            Model.Radio radioSettings = new Model.Radio
            {
                Name = radio.Name,
                RadioId = radio.RadioId,
                GpsMode = radio.GpsMode.ToString(),
                RequestInterval = radio.RequestInterval ?? 0
            };

            return radioSettings;
        }


        public static Controllers.Radio Map(Model.Radio radioSettings)
        {
            if (!Enum.TryParse(radioSettings.GpsMode, true, out GpsModeEnum gpsMode))
            {
                gpsMode = GpsModeEnum.None;
                logger.Warn($"Could not parse GpsEnum value '{radioSettings.GpsMode}', defaulted to {gpsMode.ToString()}");
            }

            Controllers.Radio radio = new Controllers.Radio
            {
                Name = radioSettings.Name,
                RadioId = radioSettings.RadioId,
                GpsMode = gpsMode,
                RequestInterval = radioSettings.RequestInterval
                
            };

            return radio;
        }

        internal static SystemSettings Map(Settings settings)
        {
            if (!Enum.TryParse(settings.DefaultGpsMode, out GpsModeEnum gpsMode))
            {
                gpsMode = GpsModeEnum.None;
            }
            Controllers.SystemSettings systemSettings = new SystemSettings
            {
                DefaultInterval = settings.DefaultInterval,
                DefaultGpsMode = gpsMode,
                ServerInterval = settings.ServerInterval,
                CiaBataSettings = new CiaBataSettings
                {
                    Host = settings.CiaBataHost
                },
                TurboNetSettings = new TurboNetSettings
                {
                    Host = settings.TrboNetHost,
                    Port = settings.TrboNetPort,
                    User = settings.TrboNetUser,
                    Password = settings.TrboNetPassword,
                }
            };

            return systemSettings;
        }
    }
}
