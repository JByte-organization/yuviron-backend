using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ArtistPayoutSettingsConfiguration : IEntityTypeConfiguration<ArtistPayoutSettings>
{
    public void Configure(EntityTypeBuilder<ArtistPayoutSettings> builder)
    {
        builder.ToTable("artist_payout_settings");

        // 1:1 связь с Artist. PK = ArtistId
        builder.HasKey(x => x.ArtistId);

        builder.Property(x => x.MinWithdrawAmount).HasPrecision(18, 2);
        builder.Property(x => x.MaxWithdrawAmount).HasPrecision(18, 2);
        builder.Property(x => x.CustomRatePerStream).HasPrecision(18, 6); // Цена за стрим дробная (0.003$)

        builder.HasOne(x => x.Artist)
               .WithOne() // Можно добавить св-во PayoutSettings в Artist
               .HasForeignKey<ArtistPayoutSettings>(x => x.ArtistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
