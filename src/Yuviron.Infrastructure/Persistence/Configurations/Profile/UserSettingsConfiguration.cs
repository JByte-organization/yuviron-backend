using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ThemeMode).IsRequired().HasMaxLength(20);    

        builder.HasOne(x => x.User)
            .WithOne(u => u.Settings)
            .HasForeignKey<UserSettings>(x => x.Id) 
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CustomTheme)
            .WithMany()
            .HasForeignKey(x => x.CustomThemeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Theme)
            .WithMany()
            .HasForeignKey(x => x.ThemeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(x => !x.User.IsDeleted);
    }
}
