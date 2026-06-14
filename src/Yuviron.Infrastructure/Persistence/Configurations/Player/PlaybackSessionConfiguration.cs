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

        builder.Property(x => x.ContextType).IsRequired().HasMaxLength(50);

        // Индексы
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.StartedAt);
        builder.HasIndex(x => new { x.ContextType, x.ContextId });

        builder.HasOne(x => x.User)
            .WithMany() 
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.User.IsDeleted);
    }
}
