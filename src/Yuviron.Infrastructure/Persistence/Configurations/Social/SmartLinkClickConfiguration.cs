using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class SmartLinkClickConfiguration : IEntityTypeConfiguration<SmartLinkClick>
{
    public void Configure(EntityTypeBuilder<SmartLinkClick> builder)
    {
        builder.ToTable("smart_link_clicks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength();
        builder.Property(x => x.DeviceType).HasMaxLength(50);
        builder.Property(x => x.Referrer).HasMaxLength(500);

        builder.HasIndex(x => x.ClickedAt);
        builder.HasIndex(x => x.SmartLinkId);

        builder.HasOne(x => x.SmartLink)
            .WithMany(s => s.Clicks)
            .HasForeignKey(x => x.SmartLinkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}