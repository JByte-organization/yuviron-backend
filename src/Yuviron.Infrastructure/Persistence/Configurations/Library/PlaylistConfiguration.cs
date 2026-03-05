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

        // Ограничения строк
        builder.Property(x => x.Title).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(2000); 
        builder.Property(x => x.CoverUrl).HasMaxLength(500);

        // Индексы для быстрого поиска
        builder.HasIndex(x => x.IsDeleted); 
        builder.HasIndex(x => x.UserId); // Чтобы быстро грузить "Мои плейлисты"

        builder.HasOne(x => x.User)
            .WithMany() 
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Удалили юзера -> удалились его плейлисты

        // Магия Soft Delete
        builder.HasQueryFilter(p => !p.IsDeleted); 
    }
}