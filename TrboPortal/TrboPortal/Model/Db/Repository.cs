using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace TrboPortal.Model.Db
{
    public class Repository
    {
        /// <summary>
        /// Generic insert or update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static async Task InsertOrUpdateAsync<T>(T entity) where T : class
        {
            using (var db = new DatabaseContext())
            {
                if (db.Entry(entity).State == EntityState.Detached)
                    db.Set<T>().Add(entity);

                // If an immediate save is needed, can be slow though
                // if iterating through many entities:
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Insert or update Radio (based on key)
        /// </summary>
        /// <param name="settings"></param>
        public static async Task InsertOrUpdateAsync(List<Radio> settings)
        {
            using (var context = new DatabaseContext())
            {
                foreach (var s in settings)
                {
                    var radioExists = await context.RadioSettings.AnyAsync(r => r.RadioId == s.RadioId);
                    context.Entry(s).State = radioExists ? EntityState.Modified : EntityState.Added;
                }

                await context.SaveChangesAsync();
            }
        }

        internal static async Task<Settings> GetLatestSystemSettingsAsync()
        {
            using (var context = new DatabaseContext())
            {
                return await context.Settings.OrderByDescending(s => s.SettingsId).FirstOrDefaultAsync();
            }
        }

        public static async Task<List<LogEntry>> GetLogging(string logLevel, long? from, long? through)
        {
            using (var context = new DatabaseContext())
            {
                return await context.LogEntries
                    .Where(l => logLevel.Equals(l.LogLevel, StringComparison.InvariantCultureIgnoreCase)
                        && from == null || from < l.Timestamp
                        && through == null || through > l.Timestamp
                    )
                    .ToListAsync();
            }
        }
    }
}