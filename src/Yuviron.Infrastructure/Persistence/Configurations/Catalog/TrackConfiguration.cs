using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("tracks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
        builder.Property(x => x.CoverUrl).HasMaxLength(2048);
        builder.Property(x => x.AudioStorageKey).IsRequired().HasMaxLength(1024); 

        builder.Property(x => x.PlayCount).HasDefaultValue(0);
        
        builder.HasOne(x => x.Album)
            .WithMany(a => a.Tracks)
            .HasForeignKey(x => x.AlbumId)
            .IsRequired() 
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}