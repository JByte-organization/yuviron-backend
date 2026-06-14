using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class CustomThemeConfiguration : IEntityTypeConfiguration<CustomTheme>
{
    public void Configure(EntityTypeBuilder<CustomTheme> builder)
    {
        builder.ToTable("custom_themes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PrimaryColor).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(x => x.SecondaryColor).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(x => x.BackgroundColor).IsRequired().HasMaxLength(7).IsFixedLength();

        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany() 
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.User.IsDeleted);
    }
}
