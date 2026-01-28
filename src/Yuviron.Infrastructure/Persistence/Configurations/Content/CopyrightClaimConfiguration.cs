using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class CopyrightClaimConfiguration : IEntityTypeConfiguration<CopyrightClaim>
{
    public void Configure(EntityTypeBuilder<CopyrightClaim> builder)
    {
        builder.ToTable("copyright_claims");
        builder.HasKey(x => x.Id);

        // Индекс для полиморфного поиска (найти жалобу по Треку или Альбому)
        builder.HasIndex(x => new { x.EntityType, x.EntityId });

        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasOne(x => x.OwnerArtist)
               .WithMany()
               .HasForeignKey(x => x.OwnerArtistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
