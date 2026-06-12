using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations.External;

public class ExternalMappingConfiguration : IEntityTypeConfiguration<ExternalMapping>
{
    public void Configure(EntityTypeBuilder<ExternalMapping> builder)
    {
        builder.ToTable("external_mappings");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.Provider, x.ExternalId, x.EntityType })
            .IsUnique();

        builder.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ExternalId).HasMaxLength(100).IsRequired();
    }
}
