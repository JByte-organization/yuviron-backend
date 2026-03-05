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

        // Ограничения для строк
        builder.Property(x => x.ReasonCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.ModerationNote).HasMaxLength(2000);

        builder.HasIndex(x => new { x.TargetType, x.TargetId }); 
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