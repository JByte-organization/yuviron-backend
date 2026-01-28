using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class PayoutRequestConfiguration : IEntityTypeConfiguration<PayoutRequest>
{
    public void Configure(EntityTypeBuilder<PayoutRequest> builder)
    {
        builder.ToTable("payout_requests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestedAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Artist)
               .WithMany()
               .HasForeignKey(x => x.ArtistId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Admin)
               .WithMany()
               .HasForeignKey(x => x.AdminId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
