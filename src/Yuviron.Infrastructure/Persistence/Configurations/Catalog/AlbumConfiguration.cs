using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class AlbumConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("albums");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.CoverUrl).HasMaxLength(2048);
        builder.Property(x => x.ReleaseType);

        builder.HasIndex(x => x.Title);
        builder.HasIndex(x => x.CreatedAt).IsDescending();
        builder.HasIndex(x => new { x.VisibilityStatus, x.CreatedAt });
        builder.HasIndex(x => x.ReleaseDate);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
