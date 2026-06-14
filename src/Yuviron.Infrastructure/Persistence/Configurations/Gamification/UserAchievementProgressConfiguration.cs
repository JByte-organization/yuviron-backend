using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserAchievementProgressConfiguration : IEntityTypeConfiguration<UserAchievementProgress>
{
    public void Configure(EntityTypeBuilder<UserAchievementProgress> builder)
    {
        builder.ToTable("user_achievement_progress");

        // КРИТИЧНО: Ограничиваем длину строки, которая участвует в первичном ключе!
        builder.Property(x => x.MetricKey).IsRequired().HasMaxLength(50);

        // Составной ключ из 3-х полей
        builder.HasKey(x => new { x.UserId, x.AchievementId, x.MetricKey });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Achievement)
            .WithMany()
            .HasForeignKey(x => x.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.User.IsDeleted);
    }
}
