using Microsoft.EntityFrameworkCore;

namespace Peskybird.App.Model;

public class PeskybirdContext : DbContext
{
    public DbSet<BotQuote> Quotes { get; set; }
    public DbSet<ChannelConfig> ChannelConfigs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        base.OnConfiguring(builder);
        builder.UseSqlite("Data Source=pesky.sqlite", x => x.MigrationsAssembly("Peskybird.Migrations"));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new ChannelConfigModel());
        builder.ApplyConfiguration(new BotQuoteConfig());
    }
}