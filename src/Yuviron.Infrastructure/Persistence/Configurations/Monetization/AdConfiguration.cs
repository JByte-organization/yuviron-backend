using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class AdConfiguration : IEntityTypeConfiguration<Ad>
{
    public void Configure(EntityTypeBuilder<Ad> builder)
    {
        builder.ToTable("ads");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdvertiserName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.AudioUrl).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ClickUrl).HasMaxLength(500);
        
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsDeleted);
        
        builder.HasQueryFilter(x => !x.IsDeleted); 
    }
}