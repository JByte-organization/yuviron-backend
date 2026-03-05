using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("artists");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Bio).HasMaxLength(2000);
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
        builder.Property(x => x.BannerUrl).HasMaxLength(500);

        builder.HasIndex(x => x.Name);

        builder.HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}