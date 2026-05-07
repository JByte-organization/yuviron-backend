using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("artists");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Bio).HasMaxLength(2000);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048); 
        builder.Property(x => x.BannerUrl).HasMaxLength(2048); 
        builder.Property(x => x.MonthlyListenersCount).HasDefaultValue(0);

        builder.HasIndex(x => x.Name);

        builder.HasIndex(x => x.CreatedAt).IsDescending();

        builder.HasIndex(x => new { x.VerificationStatus, x.CreatedAt });

        builder.HasMany(x => x.TeamMembers)
            .WithOne(x => x.Artist)
            .HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
