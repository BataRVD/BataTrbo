using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrboPortal.Controllers;

namespace TrboPortal.Model
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
