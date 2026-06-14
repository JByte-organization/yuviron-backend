using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("user_devices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Fingerprint)
            .HasMaxLength(256)
            .IsRequired(); 

        builder.Property(x => x.DeviceName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BrowserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastIpAddress)
            .HasMaxLength(45)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany() 
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Fingerprint }).IsUnique(); 

        builder.HasQueryFilter(x => !x.User.IsDeleted);
    }
}
