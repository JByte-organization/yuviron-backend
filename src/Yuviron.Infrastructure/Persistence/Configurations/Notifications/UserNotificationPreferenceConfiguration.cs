using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.ToTable("user_notification_preferences");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Category).IsRequired();
        builder.Property(x => x.Code).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Enabled).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Category, x.Code }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.User.IsDeleted);
    }
}
