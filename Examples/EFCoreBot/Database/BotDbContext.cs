using Microsoft.EntityFrameworkCore;

namespace EFCoreBot.Database;

public class BotDbContext : DbContext
{
    public BotDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}
