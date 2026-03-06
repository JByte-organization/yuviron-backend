using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("tracks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
        builder.Property(x => x.CoverUrl).HasMaxLength(500);
        builder.Property(x => x.AudioStorageKey).IsRequired().HasMaxLength(500);
        builder.Property(x => x.PreviewStorageKey).HasMaxLength(500);

        builder.HasOne(x => x.Album)
            .WithMany(a => a.Tracks)
            .HasForeignKey(x => x.AlbumId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}