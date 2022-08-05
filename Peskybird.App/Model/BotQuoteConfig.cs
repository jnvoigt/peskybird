namespace Peskybird.App.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BotQuoteConfig : IEntityTypeConfiguration<BotQuote>
{
    public void Configure(EntityTypeBuilder<BotQuote> builder)
    {
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.Quote).IsRequired();
        builder.Property(x => x.Server).IsRequired();
        builder.Property(x => x.User).IsRequired();
        builder.Property(x => x.Time).IsRequired();

        builder.ToTable("Quotes");
    }
}