using System;
using NLog;

namespace TrboPortal.Mappers
{
    public static class DateTimeMapper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static DateTime? ToDateTime(string dateTimeString)
        {
            try
            {
                return DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Unable to parse dateTimeString \"{dateTimeString}\"");
                return null;
            }
        }

        public static string ToString(DateTime? timeQueued)
        {
            return timeQueued?.ToString("o") ?? string.Empty;
        }
    }
}