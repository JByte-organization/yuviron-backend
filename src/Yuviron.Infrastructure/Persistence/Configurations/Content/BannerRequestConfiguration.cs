using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class BannerRequestConfiguration : IEntityTypeConfiguration<BannerRequest>
{
    public void Configure(EntityTypeBuilder<BannerRequest> builder)
    {
        builder.ToTable("banner_requests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(150);
        builder.Property(x => x.BannerUrl).HasMaxLength(2048);
        builder.Property(x => x.AdminNotes).HasMaxLength(1000);
        
        builder.Property(x => x.EndsAtUtc)
            .HasColumnType("datetime(6)");

        builder.Property(x => x.TargetCountries)
            .HasMaxLength(1000);

        builder.Property(x => x.TargetGenres)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.ArtistId);
        builder.HasIndex(x => x.SubmittedByUserId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Artist)
            .WithMany()
            .HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SubmittedByUser)
            .WithMany()
            .HasForeignKey(x => x.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Album)
            .WithMany()
            .HasForeignKey(x => x.AlbumId)
            .OnDelete(DeleteBehavior.SetNull); 
    }
}
