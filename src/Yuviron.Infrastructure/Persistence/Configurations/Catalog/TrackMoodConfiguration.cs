using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public sealed class TrackMoodConfiguration : IEntityTypeConfiguration<TrackMood>
{
    public void Configure(EntityTypeBuilder<TrackMood> builder)
    {
        builder.ToTable("track_moods");
        builder.HasKey(tm => new { tm.TrackId, tm.MoodId });

        builder.HasOne(tm => tm.Track)
            .WithMany(t => t.TrackMoods)
            .HasForeignKey(tm => tm.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tm => tm.Mood)
            .WithMany(m => m.TrackMoods)
            .HasForeignKey(tm => tm.MoodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(tm => !tm.Track.IsDeleted && !tm.Mood.IsDeleted);
    }
}
