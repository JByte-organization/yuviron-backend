using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class TrackGenreConfiguration : IEntityTypeConfiguration<TrackGenre>
{
    public void Configure(EntityTypeBuilder<TrackGenre> builder)
    {
        builder.ToTable("track_genres");
        builder.HasKey(x => new { x.TrackId, x.GenreId });

        builder.HasOne(x => x.Track)
               .WithMany(t => t.TrackGenres)
               .HasForeignKey(x => x.TrackId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Genre)
               .WithMany(g => g.TrackGenres)
               .HasForeignKey(x => x.GenreId)
               .OnDelete(DeleteBehavior.Restrict); // Жанр удалить нельзя, если есть треки
    }
}
