using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace TrboPortal.Model.Db
{
    public class Repository
    {
        /// <summary>
        /// Generic insert or update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static void InsertOrUpdate<T>(T entity) where T : class
        {
            using (var db = new DatabaseContext())
            {
                if (db.Entry(entity).State == EntityState.Detached)
                    db.Set<T>().Add(entity);

                // If an immediate save is needed, can be slow though
                // if iterating through many entities:
                db.SaveChanges();
            }
        }


        /// <summary>
        /// Insert or update Radio (based on key)
        /// </summary>
        /// <param name="settings"></param>
        public static void InsertOrUpdate(List<Radio> settings)
        {
            using (var context = new DatabaseContext())
            {
                foreach (var s in settings)
                {
                    bool radioExists =
                        context.RadioSettings.Any(r => r.RadioId == s.RadioId); // used to be: s.RadioId == 0

                    context.Entry(s).State = radioExists ? EntityState.Modified : EntityState.Added;
                }

                context.SaveChanges();
            }
        }

        internal static Settings GetLatestSystemSettings()
        {
            using (var context = new DatabaseContext())
            {
                return context.Settings.OrderByDescending(s => s.SettingsId).FirstOrDefault();
            }
        }

        public static List<LogEntry> GetLogging(string logLevel, long? from, long? through)
        {
            using (var context = new DatabaseContext())
            {
                return context.LogEntries
                    .Where(l => logLevel.Equals(l.LogLevel, StringComparison.InvariantCultureIgnoreCase)
//                                && from == null || from < l.Timestamp
//                                && through == null || through > l.Timestamp
                    )
                    .ToList();
            }
        }
    }
}