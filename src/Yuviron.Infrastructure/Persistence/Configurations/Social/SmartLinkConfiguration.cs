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

        builder.HasIndex(x => x.ClickedAt);

        builder.HasOne(x => x.SmartLink)
               .WithMany(s => s.Clicks)
               .HasForeignKey(x => x.SmartLinkId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}