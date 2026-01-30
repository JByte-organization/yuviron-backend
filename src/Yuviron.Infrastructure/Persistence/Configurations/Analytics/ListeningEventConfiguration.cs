using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ListeningEventConfiguration : IEntityTypeConfiguration<ListeningEvent>
{
    public void Configure(EntityTypeBuilder<ListeningEvent> builder)
    {
        builder.ToTable("listening_events");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.PlayedAt }); // История юзера
        builder.HasIndex(x => new { x.TrackId, x.PlayedAt }); // Статистика трека

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.SetNull); // Юзера нет, статистика осталась

        builder.HasOne(x => x.Track)
               .WithMany()
               .HasForeignKey(x => x.TrackId)
               .OnDelete(DeleteBehavior.Restrict); // Трек нельзя удалить, пока есть прослушивания
    }
}
