using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048); 
        
        builder.Property(x => x.Country).HasMaxLength(2).IsFixedLength();
        
        builder.Property(x => x.City).HasMaxLength(100); 

        builder.Property(x => x.Bio).HasMaxLength(1000);

        builder.Property(x => x.Gender).IsRequired();
        builder.Property(x => x.DateOfBirth).IsRequired().HasColumnType("date");

        builder.HasIndex(x => x.FirstName);

        builder.HasOne(x => x.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<UserProfile>(x => x.Id) 
            .OnDelete(DeleteBehavior.Cascade);
    }
}