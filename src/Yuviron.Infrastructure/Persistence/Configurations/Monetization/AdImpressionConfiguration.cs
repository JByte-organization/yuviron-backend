using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class AdImpressionConfiguration : IEntityTypeConfiguration<AdImpression>
{
    public void Configure(EntityTypeBuilder<AdImpression> builder)
    {
        builder.ToTable("ad_impressions");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Ad)
               .WithMany(a => a.Impressions)
               .HasForeignKey(x => x.AdId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.SetNull); // Если юзера удалили, статистика рекламы остается (как анонимная)
    }
}