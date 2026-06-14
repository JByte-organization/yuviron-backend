using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class PlaybackQueueItemConfiguration : IEntityTypeConfiguration<PlaybackQueueItem>
{
    public void Configure(EntityTypeBuilder<PlaybackQueueItem> builder)
    {
        builder.ToTable("playback_queue_items");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.SessionId, x.Position }).IsUnique();
        
        builder.HasIndex(x => x.AddedByUserId);

        builder.HasOne(x => x.Session)
            .WithMany(s => s.QueueItems)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Track)
            .WithMany()
            .HasForeignKey(x => x.TrackId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(x => x.AddedByUser)
            .WithMany()
            .HasForeignKey(x => x.AddedByUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.AddedByUser.IsDeleted && !x.Track.IsDeleted);
    }
}
