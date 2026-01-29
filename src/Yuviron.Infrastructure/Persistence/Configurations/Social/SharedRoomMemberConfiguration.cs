using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class SharedRoomMemberConfiguration : IEntityTypeConfiguration<SharedRoomMember>
{
    public void Configure(EntityTypeBuilder<SharedRoomMember> builder)
    {
        builder.ToTable("shared_room_members");
        builder.HasKey(x => new { x.RoomId, x.UserId });

        builder.HasOne(x => x.Room)
               .WithMany(r => r.Members)
               .HasForeignKey(x => x.RoomId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
