using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserSavedAlbumConfiguration : IEntityTypeConfiguration<UserSavedAlbum>
{
    public void Configure(EntityTypeBuilder<UserSavedAlbum> builder)
    {
        builder.ToTable("user_saved_albums");

        // Составной первичный ключ (Юзер + Альбом)
        builder.HasKey(x => new { x.UserId, x.AlbumId });

        builder.Property(x => x.SavedAt).IsRequired();

        // Связь с User
        builder.HasOne(x => x.User)
               .WithMany() // У юзера нет коллекции SavedAlbums (если не добавил)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Связь с Album
        builder.HasOne(x => x.Album)
               .WithMany()
               .HasForeignKey(x => x.AlbumId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}