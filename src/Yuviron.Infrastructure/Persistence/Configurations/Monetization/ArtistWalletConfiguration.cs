using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ArtistWalletConfiguration : IEntityTypeConfiguration<ArtistWallet>
{
    public void Configure(EntityTypeBuilder<ArtistWallet> builder)
    {
        builder.HasKey(x => x.Id);

        // Зв'язок 1-до-1 з Артистом
        builder.HasOne(x => x.Artist)
            .WithOne()
            .HasForeignKey<ArtistWallet>(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Гроші вимагають строгої точності
        builder.Property(x => x.AvailableBalance).HasPrecision(18, 4);
        builder.Property(x => x.HeldBalance).HasPrecision(18, 4);
        builder.Property(x => x.TotalEarned).HasPrecision(18, 4);

        // КРИТИЧНО ВАЖЛИВО: Захист від гонки (Double-spend problem)
        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}