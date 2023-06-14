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
        public DbSet<Region> Regions { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
