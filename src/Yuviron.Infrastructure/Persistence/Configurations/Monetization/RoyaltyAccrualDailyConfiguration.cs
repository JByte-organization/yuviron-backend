using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class RoyaltyAccrualDailyConfiguration : IEntityTypeConfiguration<RoyaltyAccrualDaily>
{
    public void Configure(EntityTypeBuilder<RoyaltyAccrualDaily> builder)
    {
        builder.ToTable("royalty_accruals_daily");

        // Составной ключ: Один отчет для одного артиста за одну дату
        builder.HasKey(x => new { x.ArtistId, x.Date });

        builder.Property(x => x.GrossAmount).HasPrecision(18, 6);
        builder.Property(x => x.PlatformFeeAmount).HasPrecision(18, 6);
        builder.Property(x => x.NetAmount).HasPrecision(18, 6);

        builder.HasOne(x => x.Artist)
               .WithMany()
               .HasForeignKey(x => x.ArtistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
