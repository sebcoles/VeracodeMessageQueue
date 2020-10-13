using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using VeracodeMessageQueue.Models;

namespace VeracodeMessageQueue.Storage
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<App> Apps { get; set; }
        public DbSet<Build> Builds { get; set; }
        public DbSet<Flaw> Flaws { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        public ApplicationDbContext() { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
                .AddJsonFile($"appsettings.Development.json", false)
#else
                .AddJsonFile("appsettings.json", false)
#endif
                .Build();

            var connection = Configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {        

        }
    }
}
