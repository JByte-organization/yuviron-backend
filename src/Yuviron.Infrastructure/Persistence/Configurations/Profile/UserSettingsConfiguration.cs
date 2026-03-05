using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings");
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.LanguageCode).IsRequired().HasMaxLength(10); // например, "en-US"
        builder.Property(x => x.ThemeMode).IsRequired().HasMaxLength(20);    // light, dark, system

        builder.HasOne(x => x.User)
            .WithOne(u => u.Settings)
            .HasForeignKey<UserSettings>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CustomTheme)
            .WithMany()
            .HasForeignKey(x => x.CustomThemeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}