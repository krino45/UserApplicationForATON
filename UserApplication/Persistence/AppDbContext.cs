using Microsoft.EntityFrameworkCore;
using UserApplication.API.Models;

namespace UserApplication.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
    }
}
