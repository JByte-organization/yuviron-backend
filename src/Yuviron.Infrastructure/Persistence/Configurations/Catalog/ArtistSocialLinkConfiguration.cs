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

        builder.Property(x => x.Type).IsRequired().HasMaxLength(50); // "Instagram", "Twitter"
        builder.Property(x => x.Url).IsRequired().HasMaxLength(500);

        builder.HasOne(x => x.Artist)
            .WithMany(a => a.SocialLinks)
            .HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}