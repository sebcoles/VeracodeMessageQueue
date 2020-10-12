using Microsoft.EntityFrameworkCore;
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

    }
}
