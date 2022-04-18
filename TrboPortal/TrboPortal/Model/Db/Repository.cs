using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace TrboPortal.Model.Db
{
    public class Repository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
        /// <returns>Result of SaveChangesAsync</returns>
        public static async Task<int> InsertOrUpdateRadios(List<Radio> settings)
        {
            using (var context = new DatabaseContext())
            {
                foreach (var s in settings)
                {
                    var radioExists = await context.RadioSettings.AnyAsync(r => r.RadioId == s.RadioId);
                    context.Entry(s).State = radioExists ? EntityState.Modified : EntityState.Added;
                }

                return await context.SaveChangesAsync();
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

        /// <summary>
        /// Returns RadioSettings from DB for specified Radio ids. If list is empty, will return all Radios.
        /// Prints waring for all radio's that can't be found.
        /// </summary>
        /// <param name="radioIds">Ids to return, if empty, returns all radios</param>
        /// <returns></returns>
        internal static async Task<List<Radio>> GetRadiosById(int[] radioIds)
        {
            radioIds = radioIds ?? new int[0];
            using (var context = new DatabaseContext())
            {
                var results =  await context.RadioSettings.Where(r=> radioIds.Count() == 0 || radioIds.Contains(r.RadioId)).ToListAsync();
                var missing = radioIds.Except(results.Select(r => r.RadioId)).ToList();
                if (missing.Any())
                {
                    logger.Warn($"GetRadiosById: Couldn't find RadioSettings for Radio with id(s): {string.Join(",", missing)}");
                }
                return results;
            }
        }

        internal static async Task<Radio> GetRadioById(int radioId)
        {
            var result = await GetRadiosById(new int[] { radioId });
            return result.SingleOrDefault();
        }

        public static Task DeleteRadios(IEnumerable<int> radioIds)
        {
            logger.Info($"Deleting RadioSettings for Radio IDs: {string.Join(",", radioIds)}.");
            using (var context = new DatabaseContext())
            {
                context.RadioSettings.RemoveRange(context.RadioSettings.Where(r => radioIds.Contains(r.RadioId)));
                return context.SaveChangesAsync();
            }
        }
    }
}