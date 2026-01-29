using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class LyricsConfiguration : IEntityTypeConfiguration<Lyrics>
{
    public void Configure(EntityTypeBuilder<Lyrics> builder)
    {
        builder.ToTable("lyrics");
        // 1:1 связь, PK совпадает с FK
        builder.HasKey(x => x.TrackId);

        builder.Property(x => x.PlainText).IsRequired();

        builder.HasOne(x => x.Track)
               .WithOne() // Можно добавить Track.Lyrics
               .HasForeignKey<Lyrics>(x => x.TrackId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}