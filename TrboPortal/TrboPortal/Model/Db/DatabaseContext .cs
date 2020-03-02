using Microsoft.EntityFrameworkCore;

namespace TrboPortal.Model.Db
{
    public class DatabaseContext: DbContext
    {
        public DbSet<Radio> RadioSettings { get; set; }
        public DbSet<GpsEntry> GpsEntries { get; set; }

        public DbSet<Settings> Settings { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=TrboPortal.db");
        }
    }
}
