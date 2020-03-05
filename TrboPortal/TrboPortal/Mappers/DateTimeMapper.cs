using System;
using NLog;

namespace TrboPortal.Mappers
{
    public static class DateTimeMapper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static DateTime? ToDateTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return null;
            }

            return DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        public static string ToString(DateTime? timeQueued)
        {
            return timeQueued?.ToString("o") ?? string.Empty;
        }

        public static long? ToUnixMs(string dateTimeString)
        {
            var dt = ToDateTime(dateTimeString);
            if (dt != null)
            {
                return (long) ((DateTime) dt).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            }

            return null;
        }

        public static DateTime FromUnixMs(long unixMs)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(unixMs);
        }

        public static string ToString(long unixMs)
        {
            return ToString(FromUnixMs(unixMs));
        }
    }
}