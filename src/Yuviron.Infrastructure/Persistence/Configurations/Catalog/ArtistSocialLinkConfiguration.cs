using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ArtistSocialLinkConfiguration : IEntityTypeConfiguration<ArtistSocialLink>
{
    public void Configure(EntityTypeBuilder<ArtistSocialLink> builder)
    {
        builder.ToTable("artist_social_links");
        builder.HasKey(x => x.Id);

        // Конвертируем Enum в строку для БД
        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>() 
            .HasMaxLength(50);

        builder.Property(x => x.Url).IsRequired().HasMaxLength(500);

        builder.HasOne(x => x.Artist)
            .WithMany(a => a.SocialLinks)
            .HasForeignKey(x => x.ArtistId)
            .IsRequired() // Гарантирует удаление Orphan-записей
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(x => new { x.ArtistId, x.Type }).IsUnique();
    }
}