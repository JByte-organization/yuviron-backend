using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations.Storage;

public class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {
        builder.ToTable("FileMetadatas"); 

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CurrentStorageKey).IsRequired().HasMaxLength(500);

        builder.HasIndex(x => x.UserId);
        
        builder.HasIndex(x => x.IsTemporary);
        builder.HasIndex(x => x.CreatedAt);
    }
}