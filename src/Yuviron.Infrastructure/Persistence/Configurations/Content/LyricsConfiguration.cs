using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class LyricsConfiguration : IEntityTypeConfiguration<Lyrics>
{
    public void Configure(EntityTypeBuilder<Lyrics> builder)
    {
        builder.ToTable("lyrics");
        builder.HasKey(x => x.TrackId);

        builder.Property(x => x.LanguageCode).IsRequired().HasMaxLength(10);
        builder.Property(x => x.PlainText).IsRequired(); // Остается MAX

        builder.HasOne(x => x.Track)
            .WithOne() 
            .HasForeignKey<Lyrics>(x => x.TrackId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}