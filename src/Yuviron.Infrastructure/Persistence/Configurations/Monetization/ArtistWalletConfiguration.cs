using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ArtistWalletConfiguration : IEntityTypeConfiguration<ArtistWallet>
{
    public void Configure(EntityTypeBuilder<ArtistWallet> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(w => w.Artist)
            .WithOne(a => a.ArtistWallet) 
            .HasForeignKey<ArtistWallet>(w => w.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.AvailableBalance).HasPrecision(18, 4);
        builder.Property(x => x.HeldBalance).HasPrecision(18, 4);
        builder.Property(x => x.TotalEarned).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}