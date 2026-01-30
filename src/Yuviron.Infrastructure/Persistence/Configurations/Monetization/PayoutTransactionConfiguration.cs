using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class PayoutTransactionConfiguration : IEntityTypeConfiguration<PayoutTransaction>
{
    public void Configure(EntityTypeBuilder<PayoutTransaction> builder)
    {
        builder.ToTable("payout_transactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.PayoutRequest)
               .WithMany(r => r.Transactions)
               .HasForeignKey(x => x.PayoutRequestId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
