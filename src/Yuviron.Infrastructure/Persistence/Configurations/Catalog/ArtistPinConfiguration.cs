using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ArtistPinConfiguration : IEntityTypeConfiguration<ArtistPin>
{
    public void Configure(EntityTypeBuilder<ArtistPin> builder)
    {
        builder.ToTable("artist_pins");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ArtistId, x.Position }).IsUnique(); // Уникальность позиции

        // Индекс для полиморфного поиска
        builder.HasIndex(x => new { x.EntityType, x.EntityId });

        builder.HasOne(x => x.Artist)
               .WithMany(a => a.Pins)
               .HasForeignKey(x => x.ArtistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
