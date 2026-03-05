using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.ToTable("user_blocks");
        builder.HasKey(x => x.Id);

        // Лимиты для строк
        builder.Property(x => x.ReasonCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(1000);

        // Индексы для быстрой проверки: "Забанен ли юзер сейчас?"
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsActive);

        // Явно указываем <User>, чтобы EF Core не запутался в двух связях к одной таблице
        builder.HasOne<User>(x => x.User)
            .WithMany() 
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>(x => x.BlockedByAdmin)
            .WithMany()
            .HasForeignKey(x => x.BlockedByAdminId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}