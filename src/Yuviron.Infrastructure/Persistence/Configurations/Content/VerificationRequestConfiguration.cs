using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class VerificationRequestConfiguration : IEntityTypeConfiguration<VerificationRequest>
{
    public void Configure(EntityTypeBuilder<VerificationRequest> builder)
    {
        builder.ToTable("verification_requests");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Artist)
               .WithMany() // Если добавил коллекцию в Artist: .WithMany(a => a.VerificationRequests)
               .HasForeignKey(x => x.ArtistId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SubmittedByUser)
               .WithMany()
               .HasForeignKey(x => x.SubmittedByUserId)
               .OnDelete(DeleteBehavior.Restrict); // Не удалять заявку, если удалили менеджера

        builder.HasOne(x => x.Admin)
               .WithMany()
               .HasForeignKey(x => x.AdminId)
               .OnDelete(DeleteBehavior.SetNull); // Если админа удалили, история модерации остается
    }
}