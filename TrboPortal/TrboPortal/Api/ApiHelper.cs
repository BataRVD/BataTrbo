﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using TrboPortal.Mappers;
using TrboPortal.Model;
using TrboPortal.Model.Api;
using TrboPortal.Model.Db;
using TrboPortal.TrboNet;
using GpsMeasurement = TrboPortal.Model.Api.GpsMeasurement;

namespace TrboPortal.Controllers
{
    public class ApiHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns a list of GpsMeasurements for specified devices. 
        /// </summary>
        /// <param name="ids">List of DeviceIds</param>
        /// <param name="from">Optional from filter</param>
        /// <param name="through">Optional through filter</param>
        /// <param name="limit">Limit to x amount of most recent GPSMeasurements</param>
        /// <returns></returns>
        public static async Task<List<GpsMeasurement>> GetGpsMeasurementsAsync(IEnumerable<int> ids,
            DateTime? from, DateTime? through, int limit = int.MaxValue)
        {
            if (ids == null)
            {
                ids = Enumerable.Empty<int>();
            }
            var hasNoIds = !ids.Any();
            using (var context = new DatabaseContext())
            {
                var dbResults = await context.GpsEntries
                    .Where(g => (hasNoIds || ids.Contains(g.RadioId)) &&
                                (from == null || from < g.Timestamp) &&
                                (through == null || through > g.Timestamp))
                    .OrderByDescending(g=> g.Timestamp)
                    .Take(limit)
                    .ToListAsync();

                return dbResults.Select(g => new GpsMeasurement(g)).ToList();
            }
        }

        internal static async Task<IEnumerable<Model.Api.Radio>> GetRadioSettingsAsync(int[] radioIds)
        {
            var dbRadios = await Repository.GetRadiosById(radioIds);
            var apiRadios = dbRadios.Select(dbr => EnrichRadioWithResultsAsync(DatabaseMapper.Map(dbr)));

            return apiRadios;
        }

        internal static Model.Api.Radio EnrichRadioWithResultsAsync(Model.Api.Radio radio)
        {
            if(TurboController.Instance.GetDeviceInfoByRadioID(radio.RadioId, out DeviceInfo deviceInfo)) {
                radio.LastGpsRequested = deviceInfo.LastUpdateRequest;
                // Add most recent 10 GPS Measurements

                radio.GpsMeasurements = deviceInfo.GpsLocations?.Skip(Math.Max(0, deviceInfo.GpsLocations.Count() - 10)).ToList() ?? new List<GpsMeasurement>();
                radio.Status = deviceInfo.Device?.DeviceState.ToString() ?? "Unknown";
                radio.LastSeen = deviceInfo.LastMessageReceived;
            }
            var result = GetGpsMeasurementsAsync(new List<int>() { radio.RadioId }, null, null, 10).Result;
            var latest_timestamp = result.FirstOrDefault()?.Timestamp;
            radio.LastGpsTimeStamp = latest_timestamp != null ? DateTime.Parse(latest_timestamp) : DateTime.MinValue;
            radio.GpsMeasurements = result;
            return radio;
        }

        public static async Task UpdateRadioSettingsAsync(IEnumerable<Model.Api.Radio> radioSettings)
        {
            //Store the settings
            await Repository.InsertOrUpdateRadios(radioSettings.ToList().Select(RadioMapper.MapRadioSettings).ToList());
        }

        public static async Task DeleteRadioSettings(IEnumerable<int> radioIds)
        {
            await Repository.DeleteRadios(radioIds);
        }

        internal static async Task<SystemSettings> GetSystemSettings()
        {
            // TODO: Latest from DB not the cached in the server? If we are running on default, it will return null...
            var result = await Repository.GetLatestSystemSettingsAsync();
            return DatabaseMapper.Map(result);
        }

        public static async Task UpdateSystemSettingsAsync(SystemSettings body)
        {
            if (body != null)
            {
                var newSettings = new Settings
                {
                    BataEdition = body.CiaBataSettings.Edition,
                    DefaultGpsMode = body.DefaultGpsMode?.ToString(),
                    DefaultInterval = body.DefaultInterval ?? 60,
                    ServerInterval = body.ServerInterval ?? 15,
                    TrboNetHost = body.TurboNetSettings?.Host,
                    TrboNetPort = body.TurboNetSettings?.Port ?? 1599,
                    TrboNetPassword = body.TurboNetSettings?.Password,
                    TrboNetUser = body.TurboNetSettings?.User,
                    CiaBataHost = body.CiaBataSettings?.Host,
                    CiaBataToken = body.CiaBataSettings?.Token,
                    GoogleMapsApiKey = body.GoogleMapsApiKey?.ToString(),
                };

                //Store the settings
                await Repository.InsertOrUpdateAsync(newSettings);

                // How is this for an "event driven" system? =)
                await TurboController.Instance.LoadGenericSettingsFromDatabaseAsync();
            }
        }

        internal static List<RadioException> RequestGpsUpdate(int[] radioIds)
        {
            var errors = new List<RadioException>();
            foreach (int radioID in radioIds)
            {
                try
                {
                    TurboController.Instance.PollForGps(radioID);
                }
                catch (RadioException ex)
                {
                    errors.Add(ex);
                }
            }
            return errors;
        }

        public static async Task<ICollection<LogMessage>> GetLoggingAsync(string loglevel, string from, string through)
        {
            var fromUnixMs = DateTimeMapper.ToUnixMs(from);
            var throughUnixMs = DateTimeMapper.ToUnixMs(through);
            var result = await Repository.GetLogging(loglevel, fromUnixMs, throughUnixMs);
            return result.Select(l => new LogMessage(l)).ToList();
        }


        public static QueueInfo GetQueueInfo()
        {
            var messageQueuItems = TurboController.Instance.GetRequestQueue()
                .Select(rqi => MessageQueueMapper.Map(rqi))
                .OrderBy(i => i.Timestamp)
                .ToList();
            return new QueueInfo(messageQueuItems, TurboController.LocationRequestCounter, TurboController.LocationResponseCounter, TurboController.Instance.GetInternalTrboNetQueueSize());
        }
    }
}