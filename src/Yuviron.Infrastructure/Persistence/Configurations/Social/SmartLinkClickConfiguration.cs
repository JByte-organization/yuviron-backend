using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class SmartLinkConfiguration : IEntityTypeConfiguration<SmartLink>
{
    public void Configure(EntityTypeBuilder<SmartLink> builder)
    {
        builder.ToTable("smart_links");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(32);
        builder.HasIndex(x => x.Code).IsUnique(); // Код ссылки уникален

        // Полиморфный индекс
        builder.HasIndex(x => new { x.EntityType, x.EntityId });

        builder.HasOne(x => x.CreatedByUser)
               .WithMany() // .WithMany(u => u.SmartLinks)
               .HasForeignKey(x => x.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
