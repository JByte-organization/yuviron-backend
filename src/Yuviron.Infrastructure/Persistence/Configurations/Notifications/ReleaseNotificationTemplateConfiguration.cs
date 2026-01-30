using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ReleaseNotificationTemplateConfiguration : IEntityTypeConfiguration<ReleaseNotificationTemplate>
{
    public void Configure(EntityTypeBuilder<ReleaseNotificationTemplate> builder)
    {
        builder.ToTable("release_notification_templates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TitleTemplate).IsRequired().HasMaxLength(200);
        builder.Property(x => x.BodyTemplate).IsRequired().HasMaxLength(1000);

        builder.HasOne(x => x.Artist)
               .WithMany()
               .HasForeignKey(x => x.ArtistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
