using Yuviron.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class TrackTestConfiguration : IEntityTypeConfiguration<TrackTest>
{
    public void Configure(EntityTypeBuilder<TrackTest> builder)
    {
        builder.ToTable("TrackTest");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LanguageCode).IsRequired().HasMaxLength(15);
        builder.Property(x => x.PlainText).IsRequired();

        builder.HasOne(x => x.Track)
            .WithOne() 
            .HasForeignKey<TrackTest>(x => x.TrackId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



