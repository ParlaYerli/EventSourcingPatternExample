using EventSourcing.API.Context.DTO;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.API.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
    }
}
