using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Wallet)
            .WithMany()
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.Description).HasMaxLength(500);

        // Індекс для швидкої вибірки історії транзакцій
        builder.HasIndex(x => new { x.WalletId, x.CreatedAt });
    }
}