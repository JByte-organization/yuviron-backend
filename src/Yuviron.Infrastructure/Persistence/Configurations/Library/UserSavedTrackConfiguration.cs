using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserSavedTrackConfiguration : IEntityTypeConfiguration<UserSavedTrack>
{
    public void Configure(EntityTypeBuilder<UserSavedTrack> builder)
    {
        builder.ToTable("user_saved_tracks");
        builder.HasKey(x => new { x.UserId, x.TrackId });

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Track)
               .WithMany()
               .HasForeignKey(x => x.TrackId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
