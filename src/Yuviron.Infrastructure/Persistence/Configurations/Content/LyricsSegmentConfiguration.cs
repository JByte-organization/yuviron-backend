using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class LyricsSegmentConfiguration : IEntityTypeConfiguration<LyricsSegment>
{
    public void Configure(EntityTypeBuilder<LyricsSegment> builder)
    {
        builder.ToTable("lyrics_segments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text).IsRequired().HasMaxLength(500);

        builder.HasIndex(x => new { x.TrackId, x.StartMs }); 

        builder.HasOne(x => x.Track)
            .WithMany()
            .HasForeignKey(x => x.TrackId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}