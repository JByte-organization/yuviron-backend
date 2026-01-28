using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserFollowArtistConfiguration : IEntityTypeConfiguration<UserFollowArtist>
{
    public void Configure(EntityTypeBuilder<UserFollowArtist> builder)
    {
        builder.ToTable("user_follow_artists");

        // Составной первичный ключ (Юзер + Артист)
        builder.HasKey(x => new { x.UserId, x.ArtistId });

        builder.Property(x => x.FollowedAt).IsRequired();
        builder.Property(x => x.NotifyNewReleases).IsRequired();

        // Связь с User
        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Связь с Artist
        builder.HasOne(x => x.Artist)
               .WithMany() // У артиста нет коллекции подписчиков в этом классе (опционально)
               .HasForeignKey(x => x.ArtistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
