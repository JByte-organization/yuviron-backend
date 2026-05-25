using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ArtistSubscriptionConfiguration : IEntityTypeConfiguration<ArtistSubscription>
{
    public void Configure(EntityTypeBuilder<ArtistSubscription> builder)
    {
        builder.ToTable("artist_subscriptions");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ArtistId);
        builder.HasIndex(x => x.PayerUserId);
        builder.HasIndex(x => x.EndAt); 

        builder.HasOne(x => x.Artist)
            .WithMany(a => a.Subscriptions) 
            .HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.HasOne(x => x.PayerUser)
            .WithMany() 
            .HasForeignKey(x => x.PayerUserId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}