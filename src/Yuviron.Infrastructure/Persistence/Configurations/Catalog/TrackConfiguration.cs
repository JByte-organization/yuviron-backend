using System;
using System.Collections.Generic;
using System.Text;
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
        builder.Property(x => x.AudioStorageKey).IsRequired();

        // Если альбом удалили, трек остается "синглом" (SetNull)
        builder.HasOne(x => x.Album)
               .WithMany(a => a.Tracks)
               .HasForeignKey(x => x.AlbumId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}