using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class AdImpressionConfiguration : IEntityTypeConfiguration<AdImpression>
{
    public void Configure(EntityTypeBuilder<AdImpression> builder)
    {
        builder.ToTable("ad_impressions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Context).HasMaxLength(100);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ShownAt);

        builder.HasOne(x => x.Ad)
            .WithMany(a => a.Impressions)
            .HasForeignKey(x => x.AdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}