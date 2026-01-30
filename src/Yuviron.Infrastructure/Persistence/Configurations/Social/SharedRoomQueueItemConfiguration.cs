using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class SharedRoomQueueItemConfiguration : IEntityTypeConfiguration<SharedRoomQueueItem>
{
    public void Configure(EntityTypeBuilder<SharedRoomQueueItem> builder)
    {
        builder.ToTable("shared_room_queue");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.RoomId, x.Position }).IsUnique();

        builder.HasOne(x => x.Room)
               .WithMany(r => r.Queue)
               .HasForeignKey(x => x.RoomId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Track)
               .WithMany()
               .HasForeignKey(x => x.TrackId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AddedByUser)
               .WithMany()
               .HasForeignKey(x => x.AddedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
