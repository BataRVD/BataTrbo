using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using SQLite.CodeFirst;
using TrboPortal.Model.Db;

namespace TrboPortal.Model
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Radio> RadioSettings { get; set; }
        public DbSet<GpsEntry> GpsEntries { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }

        public DatabaseContext(SQLiteConnection conn) : base(conn, true)
        {
            //Open connection in constructor (will be closed in base on Dispose).
            conn.Open();
            Configure();
        }

        private static string GetDatabasePath()
        {
            var dbPath = ConfigurationManager.AppSettings["DatabasePath"];
            if (string.IsNullOrEmpty(dbPath))
            {
                dbPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\TrboPortal.db";
            }
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }
            return dbPath;
    }

        public DatabaseContext() : this(new SQLiteConnection($"Data Source={GetDatabasePath()}"))
        {
            //Nothing to do
        }

        private void Configure()
        {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ConfigureModels(modelBuilder);
            var sqliteConnectionInitializer = new SqliteDropCreateDatabaseWhenModelChanges<DatabaseContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        private void ConfigureModels(DbModelBuilder mb)
        {
            mb.Entity<GpsEntry>().ToTable("GpsEntry");
            // Add Index (and Unique Constraint) on (RadioId, Timestamp), preventing duplicate inserts of same GpsMeasurement.
            mb.Entity<GpsEntry>().HasIndex(entity => new { entity.RadioId, entity.Timestamp }).IsUnique();
            mb.Entity<Radio>().ToTable("Radio");
            mb.Entity<Settings>().ToTable("Settings");
            mb.Entity<LogEntry>().ToTable("LogEntry");
        }
    }
}