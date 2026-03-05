using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class AlbumConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("albums");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Description).HasMaxLength(2000); // Ограничиваем био альбома
        builder.Property(x => x.CoverUrl).HasMaxLength(500);

        builder.HasIndex(x => x.ReleaseDate);
    }
}