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
        builder.HasKey(x => x.UserId); // 1:1

        builder.HasOne(x => x.User)
               .WithOne(u => u.Settings)
               .HasForeignKey<UserSettings>(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CustomTheme)
               .WithMany()
               .HasForeignKey(x => x.CustomThemeId)
               .OnDelete(DeleteBehavior.SetNull); // Если тему удалили, настройки сбрасываются в дефолт
    }
}