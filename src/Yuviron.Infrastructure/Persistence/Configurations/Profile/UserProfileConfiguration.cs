using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        // PK совпадает с FK (1:1 связь)
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(2); // ISO code

        builder.HasOne(x => x.User)
               .WithOne(u => u.Profile)
               .HasForeignKey<UserProfile>(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
