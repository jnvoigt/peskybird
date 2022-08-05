namespace Peskybird.App.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ChannelConfigModel : IEntityTypeConfiguration<ChannelConfig>
{
    public void Configure(EntityTypeBuilder<ChannelConfig> builder)
    {
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.Server).IsRequired();
        builder.Property(x => x.Category).IsRequired();
        builder.Property(x => x.NameGenerator).IsRequired();
        builder.ToTable("ChannelConfigs");
    }
}