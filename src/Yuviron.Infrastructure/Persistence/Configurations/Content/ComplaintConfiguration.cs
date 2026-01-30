using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.ToTable("complaints");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TargetType, x.TargetId }); // Полиморфный поиск
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.CreatedByUser)
               .WithMany()
               .HasForeignKey(x => x.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ModeratedByAdmin)
               .WithMany()
               .HasForeignKey(x => x.ModeratedByAdminId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
