using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class TrackListenHeatmapConfiguration : IEntityTypeConfiguration<TrackListenHeatmap>
{
    public void Configure(EntityTypeBuilder<TrackListenHeatmap> builder)
    {
        builder.ToTable("track_listen_heatmap");
        
        // Композитный первичный ключ
        builder.HasKey(x => new { x.TrackId, x.SecondIndex });

        builder.HasOne(x => x.Track)
            .WithMany() 
            .HasForeignKey(x => x.TrackId)
            .OnDelete(DeleteBehavior.Cascade); // Удалили трек из базы — удалили и его тепловую карту
    }
}