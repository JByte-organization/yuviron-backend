using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    public void Configure(EntityTypeBuilder<Theme> builder)
    {
        builder.ToTable("themes");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).IsRequired().HasMaxLength(64);
        builder.Property(x => x.PrimaryColor).IsRequired().HasMaxLength(32);
        builder.Property(x => x.SecondaryColor).IsRequired().HasMaxLength(32);
        builder.Property(x => x.BackgroundColor).IsRequired().HasMaxLength(32);
        builder.Property(x => x.UserId).IsRequired(false);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
    }
}
