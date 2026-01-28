using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class PlaylistConfiguration : IEntityTypeConfiguration<Playlist>
{
    public void Configure(EntityTypeBuilder<Playlist> builder)
    {
        builder.ToTable("playlists");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(150);
        builder.HasIndex(x => x.IsDeleted); // Для быстрого фильтра soft-delete

        builder.HasOne(x => x.User)
               .WithMany() // Если нужно, добавь в User: ICollection<Playlist> Playlists
               .HasForeignKey(x => x.OwnerUserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(p => !p.IsDeleted); // Автоматически скрывать удаленные
    }
}
