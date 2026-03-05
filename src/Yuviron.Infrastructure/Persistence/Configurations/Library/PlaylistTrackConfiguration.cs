using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class PlaylistTrackConfiguration : IEntityTypeConfiguration<PlaylistTrack>
{
    public void Configure(EntityTypeBuilder<PlaylistTrack> builder)
    {
        builder.ToTable("playlist_tracks");
        
        builder.HasKey(x => new { x.PlaylistId, x.TrackId });

        builder.HasIndex(x => x.TrackId);

        builder.HasOne(x => x.Playlist)
            .WithMany(p => p.PlaylistTracks)
            .HasForeignKey(x => x.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Track)
            .WithMany()
            .HasForeignKey(x => x.TrackId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasQueryFilter(pt => !pt.Playlist.IsDeleted);
    }
}