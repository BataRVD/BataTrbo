﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrboPortal.Model;
using TrboPortal.Controllers;
using NLog;

namespace TrboPortal.Mappers
{
    public static class DatabaseMappercs
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Model.Radio Map(Controllers.Radio radio)
        {
            Model.Radio radioSettings = new Model.Radio
            {
                Name = radio.Name,
                RadioId = radio.RadioId,
                GpsMode = radio.RadioSettings.GpsMode.ToString(),
                RequestInterval = radio.RadioSettings.RequestInterval ?? 0
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
                RadioSettings = new Controllers.RadioSettings
                {
                    GpsMode = gpsMode,
                    RadioId = radioSettings.RadioId,
                    RequestInterval = radioSettings.RequestInterval
                }
            };

            return radio;
        }

    }
}