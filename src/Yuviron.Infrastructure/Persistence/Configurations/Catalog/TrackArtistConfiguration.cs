using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class TrackArtistConfiguration : IEntityTypeConfiguration<TrackArtist>
{
    public void Configure(EntityTypeBuilder<TrackArtist> builder)
    {
        builder.ToTable("track_artists");
        builder.HasKey(x => new { x.TrackId, x.ArtistId });

        builder.HasOne(x => x.Track)
               .WithMany(t => t.TrackArtists)
               .HasForeignKey(x => x.TrackId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Artist)
               .WithMany(a => a.TrackArtists)
               .HasForeignKey(x => x.ArtistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}