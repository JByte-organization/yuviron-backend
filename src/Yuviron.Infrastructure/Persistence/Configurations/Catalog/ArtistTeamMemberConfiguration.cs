using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class ArtistTeamMemberConfiguration : IEntityTypeConfiguration<ArtistTeamMember>
{
    public void Configure(EntityTypeBuilder<ArtistTeamMember> builder)
    {
        builder.ToTable("artist_team_members");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
            .IsRequired();

        builder.HasIndex(x => new { x.ArtistId, x.UserId })
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany() 
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}