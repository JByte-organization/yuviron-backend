using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations.Outbox;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(500); 

        builder.Property(x => x.Content)
            .IsRequired(); 

        builder.Property(x => x.Error)
            .HasMaxLength(2000); 
            
        builder.HasIndex(x => new { x.ProcessedOnUtc, x.NextAttemptUtc, x.OccurredOnUtc });
    }
}