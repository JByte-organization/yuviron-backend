using System;
using System.Collections.Generic;
using System.Text;

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

        // Связь с заблокированным пользователем
        builder.HasOne(x => x.User)
            .WithMany() // У юзера может и не быть коллекции Blocks в классе, если не надо
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь с админом (он тоже User)
        builder.HasOne(x => x.BlockedByAdmin)
            .WithMany()
            .HasForeignKey(x => x.BlockedByAdminId)
            .OnDelete(DeleteBehavior.Restrict); // Нельзя удалить админа, если он кого-то банил (или SetNull)
    }
}