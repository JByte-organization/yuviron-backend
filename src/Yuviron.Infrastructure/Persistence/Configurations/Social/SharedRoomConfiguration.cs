using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class SharedRoomConfiguration : IEntityTypeConfiguration<SharedRoom>
{
    public void Configure(EntityTypeBuilder<SharedRoom> builder)
    {
        builder.ToTable("shared_rooms");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.HostUser)
               .WithMany()
               .HasForeignKey(x => x.HostUserId)
               .OnDelete(DeleteBehavior.Restrict); // Удаление хоста не должно молча сносить активную комнату
    }
}
