using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using TrboPortal.Mappers;
using TrboPortal.Model;
using TrboPortal.Model.Api;
using TrboPortal.Model.Db;
using TrboPortal.TrboNet;
using GpsMeasurement = TrboPortal.Model.Api.GpsMeasurement;

namespace TrboPortal.Controllers
{
    public class TrboPortalHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns a list of GpsMeasurements for specified devices. 
        /// </summary>
        /// <param name="ids">List of DeviceIds</param>
        /// <param name="from">Optional from filter</param>
        /// <param name="through">Optional through filter</param>
        /// <returns></returns>
        public static ICollection<GpsMeasurement> GetGpsMeasurements(IEnumerable<int> ids,
            DateTime? from, DateTime? through)
        {
            bool hasNoIds = (ids == null || ids.Count() == 0);
            using (var context = new DatabaseContext())
            {
                return context.GpsEntries
                    .Where(g => g.RadioId.HasValue &&
                                (hasNoIds || ids.Contains(g.RadioId.Value)) &&
                                (from == null || from < g.Timestamp) &&
                                (through == null || through > g.Timestamp))
                    .ToList() // Execute the query
                    .Select(g => new GpsMeasurement(g))
                    .ToList();
            }
        }

        public static void UpdateRadioSettings(IEnumerable<Model.Api.Radio> radioSettings)
        {
            //Store the settings
            Repository.InsertOrUpdate(radioSettings.ToList().Select(RadioMapper.MapRadioSettings).ToList());

            // How is this for an "event driven" system? =)
            TurboController.Instance.loadRadioSettingsFromDatabase();

        }

        internal static SystemSettings GetSystemSettings()
        {
            // TODO: Latest from DB not the cached in the server? If we are running on default, it will return null...
            return DatabaseMapper.Map(Repository.GetLatestSystemSettings());
        }

        public static void UpdateSystemSettings(SystemSettings body)
        {
            if (body != null)
            {
                var newSettings = new Settings
                {
                    DefaultGpsMode = body.DefaultGpsMode?.ToString(),
                    DefaultInterval = body.DefaultInterval ?? 60,
                    ServerInterval = body.ServerInterval ?? 15,
                    TrboNetHost = body.TurboNetSettings?.Host,
                    TrboNetPort = body.TurboNetSettings?.Port ?? 1599,
                    TrboNetPassword = body.TurboNetSettings?.Password,
                    TrboNetUser = body.TurboNetSettings?.User,
                    CiaBataHost = body.CiaBataSettings?.Host,
                };

                //Store the settings
                Repository.InsertOrUpdate(newSettings);

                // How is this for an "event driven" system? =)
                TurboController.Instance.loadGenericSettingsFromDatabase();
            }
        }
    }
}