using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class PlaylistTrackConfiguration : IEntityTypeConfiguration<PlaylistTrack>
{
    public void Configure(EntityTypeBuilder<PlaylistTrack> builder)
    {
        builder.ToTable("playlist_tracks");
        // Составной ключ: Плейлист + Трек. 
        // Внимание: Если хочешь разрешить дубликаты трека в плейлисте, добавь Id и сделай его PK.
        // Но пока делаем классически: трек в плейлисте 1 раз.
        builder.HasKey(x => new { x.PlaylistId, x.TrackId });

        builder.HasOne(x => x.Playlist)
               .WithMany(p => p.PlaylistTracks)
               .HasForeignKey(x => x.PlaylistId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Track)
               .WithMany()
               .HasForeignKey(x => x.TrackId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
