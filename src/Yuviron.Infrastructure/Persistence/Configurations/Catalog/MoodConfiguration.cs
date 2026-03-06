using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public sealed class MoodConfiguration : IEntityTypeConfiguration<Mood>
{
    public void Configure(EntityTypeBuilder<Mood> builder)
    {
        builder.ToTable("moods");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.CoverUrl)
            .HasMaxLength(2000);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}