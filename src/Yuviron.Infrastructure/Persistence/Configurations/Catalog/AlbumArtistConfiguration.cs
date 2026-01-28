using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class AlbumArtistConfiguration : IEntityTypeConfiguration<AlbumArtist>
{
    public void Configure(EntityTypeBuilder<AlbumArtist> builder)
    {
        builder.ToTable("album_artists");
        builder.HasKey(x => new { x.AlbumId, x.ArtistId });

        builder.HasOne(x => x.Album)
               .WithMany(a => a.AlbumArtists)
               .HasForeignKey(x => x.AlbumId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Artist)
               .WithMany(a => a.AlbumArtists)
               .HasForeignKey(x => x.ArtistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
