using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NLog;
using TrboPortal.Mappers;
using TrboPortal.Model;
using TrboPortal.TrboNet;

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
            using var context = new DatabaseContext();
            return context.GpsEntries
                .Where(g => ids.Contains(g.RadioId) &&
                            (from == null || from < g.Timestamp) &&
                            (through == null || through > g.Timestamp))
                .Select(g => new GpsMeasurement(g))
                .ToList();
        }

        public static void UpdateRadioSettings(IEnumerable<RadioSettings> radioSettings)
        {
            //Store the settings
            Repository.InsertOrUpdate(radioSettings.ToList().Select(RadioMapper.MapRadioSettings).ToList());

            //TODO JV: Dit gaat dus niet meer werken, sowieso is die methode een noop geworden sinds 134fa25a 
             radioSettings?.ToList().ForEach(d => { TurboController.Instance.AddOrUpdateDeviceSettings(d); });

            //TODO Fire settings changed event?

        }

        public static void UpdateSystemSettings(SystemSettings body)
        {
            var newSettings = new Settings
            {
                DefaultGpsMode = body.DefaultGpsMode?.ToString(),
                DefaultInterval = body.DefaultInterval ?? 60,
                ServerInterval = body.ServerInterval ?? 15,
                TrboNetHost = body.Settings?.Host,
                TrboNetPort = body.Settings?.Port ?? 1599,
                TrboNetPassword = body.Settings?.Password,
                TrboNetUser = body.Settings?.User
            };

            //Store the settings
            Repository.InsertOrUpdate(newSettings);

            //TODO: Fire event settings changed?
        }
    }
}