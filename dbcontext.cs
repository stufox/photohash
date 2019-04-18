using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;

namespace photohash
{
    public class photohashContext: DbContext
    {
        public DbSet<photoObject> photoObjects {get;set;}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./hashes.db");
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging(true);
        }
    }
}