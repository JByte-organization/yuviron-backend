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
        
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Role).IsRequired();

        builder.HasIndex(x => new { x.ArtistId, x.UserId }).IsUnique();

        builder.Property<int?>("OwnerUniqueConstraint")
            .HasComputedColumnSql("CASE WHEN Role = 1 THEN 1 ELSE NULL END", stored: true);

        builder.HasIndex("ArtistId", "OwnerUniqueConstraint").IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(u => u.ManagedArtists) 
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.Artist)
            .WithMany(a => a.TeamMembers)
            .HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}