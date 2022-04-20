using System;
using NLog;
using TrboPortal.Model.Api;
using TrboPortal.Model.Db;
using GpsMeasurement = TrboPortal.Model.Api.GpsMeasurement;
using Radio = TrboPortal.Model.Api.Radio;

namespace TrboPortal.Mappers
{
    public static class DatabaseMapper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        # region ==================== API -> DB Mappers ====================
        public static Model.Db.Radio Map(Radio radio)
        {
            Model.Db.Radio radioSettings = new Model.Db.Radio
            {
                Name = radio.Name,
                RadioId = radio.RadioId,
                GpsMode = radio.GpsMode.ToString(),
                RequestInterval = radio.RequestInterval ?? 0
            };

            return radioSettings;
        }

        public static GpsEntry Map(GpsMeasurement gpsMeasurement)
        {
            GpsEntry entry = new GpsEntry
            {
                RadioId = gpsMeasurement.RadioID,
                DeviceId = gpsMeasurement.DeviceID,
                Latitude = gpsMeasurement.Latitude,
                Longitude = gpsMeasurement.Longitude,
                Rssi = gpsMeasurement.Rssi,
                Timestamp = DateTimeMapper.ToDateTime(gpsMeasurement.Timestamp) ?? DateTime.Now,
            };

            return entry;
        }

        #endregion ==================== API -> DB Mappers ====================

        #region ==================== DB -> API Mappers ====================

        public static Radio Map(Model.Db.Radio radioSettings)
        {
            Radio radio = new Radio
            {
                Name = radioSettings.Name,
                RadioId = radioSettings.RadioId,
                GpsMode = MapGpsMode(radioSettings.GpsMode),
                RequestInterval = radioSettings.RequestInterval
            };

            return radio;
        }

        public static GpsModeEnum MapGpsMode(string gpsModeString)
        {
            if (!Enum.TryParse(gpsModeString, true, out GpsModeEnum gpsMode))
            {
                gpsMode = GpsModeEnum.None;
                logger.Warn($"Could not parse GpsEnum value '{gpsModeString}', defaulted to {gpsMode}");
            }
            return gpsMode;
        }

        internal static SystemSettings Map(Settings settings)
        {
            if (settings == null)
            {
                return null;
            }
            if (!Enum.TryParse(settings.DefaultGpsMode, out GpsModeEnum gpsMode))
            {
                gpsMode = GpsModeEnum.None;
            }
            SystemSettings systemSettings = new SystemSettings
            {
                DefaultInterval = settings.DefaultInterval,
                DefaultGpsMode = gpsMode,
                ServerInterval = settings.ServerInterval,
                GoogleMapsApiKey = settings.GoogleMapsApiKey,
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

        #endregion ==================== DB -> API Mappers ====================

    }
}
