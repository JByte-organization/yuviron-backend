using System;
using System.Collections.Generic;
using System.Text;
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

        builder.HasOne(x => x.User)
               .WithMany() // В User нет коллекции тем (не обязательно)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
