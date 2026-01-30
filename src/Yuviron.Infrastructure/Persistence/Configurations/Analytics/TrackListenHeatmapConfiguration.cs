using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class TrackListenHeatmapConfiguration : IEntityTypeConfiguration<TrackListenHeatmap>
{
    public void Configure(EntityTypeBuilder<TrackListenHeatmap> builder)
    {
        builder.ToTable("track_listen_heatmap");
        builder.HasKey(x => new { x.TrackId, x.SecondIndex });

        builder.HasOne(x => x.Track)
               .WithMany() // Можно добавить Track.Heatmap
               .HasForeignKey(x => x.TrackId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}