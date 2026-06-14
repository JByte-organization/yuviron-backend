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

        // --- Ограничения колонок (Критично для больших таблиц!) ---
        builder.Property(x => x.DeviceType).HasMaxLength(50);
        builder.Property(x => x.SourceType).HasMaxLength(50);
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength(); // Ровно 2 символа

        // --- Индексы ---
        builder.HasIndex(x => x.PlayedAt); 
        builder.HasIndex(x => new { x.UserId, x.PlayedAt }); 
        builder.HasIndex(x => new { x.TrackId, x.PlayedAt }); 

        // --- Связи ---
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull); 

        // Так как у трека есть Soft Delete (IsDeleted), Restrict здесь идеален.
        // Физически удалять трек мы не будем, поэтому конфликт FK не возникнет.
        builder.HasOne(x => x.Track)
            .WithMany()
            .HasForeignKey(x => x.TrackId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}
