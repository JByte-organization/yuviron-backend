using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserSavedPlaylistonfiguration : IEntityTypeConfiguration<UserSavedPlaylist>
{
    public void Configure(EntityTypeBuilder<UserSavedPlaylist> builder)
    {
        builder.ToTable("user_saved_playlist");

        builder.HasKey(x => new { x.UserId, x.PlaylistId });

        builder.Property(x => x.SavedAt).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Playlist)
            .WithMany()
            .HasForeignKey(x => x.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}