using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class PlaybackSessionConfiguration : IEntityTypeConfiguration<PlaybackSession>
{
    public void Configure(EntityTypeBuilder<PlaybackSession> builder)
    {
        builder.ToTable("playback_sessions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContextType).HasMaxLength(50);

        // Индекс для полиморфного контекста
        builder.HasIndex(x => new { x.ContextType, x.ContextId });

        builder.HasOne(x => x.User)
               .WithMany() // .WithMany(u => u.Sessions)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
