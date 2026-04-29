using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("banners");

        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Id)
            .ValueGeneratedNever(); 

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.BannerUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(b => b.TargetUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(b => b.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        builder.HasIndex(b => new { b.IsActive, b.SortOrder });
    }
}