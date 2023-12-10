using Microsoft.EntityFrameworkCore;

namespace DependencyInjection.Database;

public class BotDbContext : DbContext
{
    public BotDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}