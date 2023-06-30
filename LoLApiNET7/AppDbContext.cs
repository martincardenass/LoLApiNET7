using LoLApiNET7.Models;
using Microsoft.EntityFrameworkCore;

namespace LoLApiNET7
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<Champion> Champions { get; set; }
        public DbSet<ChampionInfo> ChampionsInfo { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<RegionChampionsCount> RegionChampionsCount { get; set; } // Get the view created that counts the amount of champions in each region
        public DbSet<RoleChampionsCount> RoleChampionsCount { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegionChampionsCount>().HasNoKey(); // RegionChampionsCount is a view that does not have a key.
            modelBuilder.Entity<RoleChampionsCount>().HasNoKey(); // Same as above
        }
    }
}
