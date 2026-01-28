using System;
using System.Collections.Generic;
using System.Text;
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

        // Уникальность позиции внутри сессии
        builder.HasIndex(x => new { x.SessionId, x.Position }).IsUnique();

        builder.HasOne(x => x.Session)
               .WithMany(s => s.QueueItems)
               .HasForeignKey(x => x.SessionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Track)
               .WithMany()
               .HasForeignKey(x => x.TrackId)
               .OnDelete(DeleteBehavior.Restrict); // Если удалить трек, очередь не ломается (или Cascade, по вкусу)

        builder.HasOne(x => x.AddedByUser)
               .WithMany()
               .HasForeignKey(x => x.AddedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
