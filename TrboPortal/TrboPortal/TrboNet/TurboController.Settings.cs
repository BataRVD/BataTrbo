﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TrboPortal.Mappers;
using TrboPortal.Model;
using TrboPortal.Model.Api;
using TrboPortal.Model.Db;
using Radio = TrboPortal.Model.Api.Radio;

namespace TrboPortal.TrboNet
{
    /// <summary>
    /// Storing and retrieving settings
    /// </summary>
    public sealed partial class TurboController
    {
        // database
        private static readonly DatabaseContext DbContext = new DatabaseContext();

        // Config section
        private static string ciaBataUrl;
        private static int serverInterval;
        private static string turboNetUrl;
        private static int turboNetPort;
        private static string turboNetUser;
        private static string turboNetPassword;
        private static GpsModeEnum defaultGpsMode;
        private static int defaultRequestInterval;

        // In memory state of the server
        // This is a dictionary with RadioID --> Settings
        private static ConcurrentDictionary<int, Radio> radios = new ConcurrentDictionary<int, Radio>();

        private static void LoadDefaultSettings()
        {
            var settings = new SystemSettings();
            serverInterval = Math.Max(250, settings.ServerInterval ?? 0);
            ciaBataUrl = settings.CiaBataSettings?.Host;

            turboNetUrl = settings.TurboNetSettings?.Host;
            turboNetPort = settings.TurboNetSettings?.Port ?? 0;
            turboNetUser = settings.TurboNetSettings?.User;
            turboNetPassword = settings.TurboNetSettings?.Password;

            defaultGpsMode = GpsModeEnum.None;
            defaultRequestInterval = 60;
        }

        private async Task LoadSettingsFromDatabaseAsync()
        {
            try
            {
                await LoadRadioSettingsFromDatabaseAsync();
                await LoadGenericSettingsFromDatabaseAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Could not load the settings from the database");
            }
        }

        public async Task LoadGenericSettingsFromDatabaseAsync()
        {
            try
            {
                var settings = await Repository.GetLatestSystemSettingsAsync();
                if (settings != null)
                {
                    serverInterval = Math.Max(250, settings.ServerInterval);
                    ciaBataUrl = settings.CiaBataHost;

                    turboNetUrl = settings.TrboNetHost;
                    turboNetPort = settings.TrboNetPort;
                    turboNetUser = settings.TrboNetUser;
                    turboNetPassword = settings.TrboNetPassword;

                    // Invalidate connection to make TrboNet use new config
                    Connected = false;
                    // Update Ciabata
                    ciaBataController.url = ciaBataUrl;

                    // update heartbeat
                    heartBeat.Interval = serverInterval;

                    if (!Enum.TryParse(settings.DefaultGpsMode, out defaultGpsMode))
                    {
                        defaultGpsMode = GpsModeEnum.None;
                    }

                    defaultRequestInterval = settings.DefaultInterval;
                }
                else
                {
                    logger.Info("No generic settings in the database");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Could not load the generic settings from the database");
            }
        }


        public async Task LoadRadioSettingsFromDatabaseAsync()
        {
            try
            {
                var dbSettings = await DbContext.RadioSettings.ToListAsync(); // Execute query

                var radiosFromSettings = dbSettings
                    .Select(rs => DatabaseMapper.Map(rs))
                    .ToDictionary(r => r.RadioId, r => r);

                foreach (var radio in radiosFromSettings)
                {
                    radios.AddOrUpdate(radio.Key, radio.Value, (rid, oldvalue) => { return radio.Value; });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Could not load the radio settings from the database");
            }
        }

        private bool GetRadioByRadioID(int radioID, out Radio radio)
        {
            bool radioFound = radios.TryGetValue(radioID, out radio);
            if (!radioFound)
            {
                logger.Warn($"Can't find radio settings for radioID {radioID}");
            }

            return radioFound;
        }

        public List<Radio> GetRadioSettings(IEnumerable<int> radioIds)
        {
            var selectedRadios =
                radios.Values.Where(r => (radioIds == null || !radioIds.Any()) || radioIds.Contains(r.RadioId));
            return new List<Radio>(selectedRadios);
        }
    }
}