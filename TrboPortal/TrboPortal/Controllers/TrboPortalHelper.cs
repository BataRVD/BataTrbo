using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
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
        public static ICollection<GpsMeasurement> GetGpsMeasurements(IEnumerable<int> ids, DateTime? from, DateTime? through)
        {
            return TurboController.Instance.GetDevices()
                .Where(d => ids.Contains(d.Id))
                .SelectMany(d => d.GpsLocations
                    .Where(m => (from == null || from < m.TimestampDateTime) &&
                                (through == null || through > m.TimestampDateTime))
                    .ToList())
                .ToList();
        }

        


    }
}