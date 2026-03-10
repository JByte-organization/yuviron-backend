using Yuviron.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class TrackTestConfiguration : IEntityTypeConfiguration<TrackTest>
{
    public void Configure(EntityTypeBuilder<TrackTest> builder)
    {
        builder.ToTable("TrackTests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(500);
    }
}