using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Exception = System.Exception;

namespace TrboPortal.Controllers
{
    public partial class GpsMeasurement
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DateTime? TimestampDateTime
        {
            get
            {
                try
                {
                    return DateTime.Parse(Timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Unable to parse GpsMeasurement Timestamp \"{Timestamp}\" of  Radio {RadioID}");
                    return null;
                }
            }
        }
    }
}