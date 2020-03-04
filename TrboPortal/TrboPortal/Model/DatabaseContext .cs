using System.Data.Entity;
using System.Data.SQLite;
using SQLite.CodeFirst;
using TrboPortal.Model.Db;

namespace TrboPortal.Model
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Radio> RadioSettings { get; set; }
        public DbSet<GpsEntry> GpsEntries { get; set; }
        public DbSet<Settings> Settings { get; set; }

        public DatabaseContext(SQLiteConnection conn) : base(conn, true)
        {
            //Open connection in constructor (will be closed in base on Dispose).
            conn.Open();
            Configure();
        }

        public DatabaseContext() : this(new SQLiteConnection("Data Source=|DataDirectory|TrboNet.db"))
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
            mb.Entity<Radio>().ToTable("Radio");
            mb.Entity<Settings>().ToTable("Settings");
        }
    }
}