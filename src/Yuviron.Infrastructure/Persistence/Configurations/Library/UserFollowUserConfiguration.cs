using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserFollowUserConfiguration: IEntityTypeConfiguration<UserFollowUser>
{
    public void Configure(EntityTypeBuilder<UserFollowUser> builder)
    {
        builder.ToTable("user_follow_user");

        builder.HasKey(x => new { x.FollowerId, x.FolloweeId });
        
        builder.HasOne(x => x.Follower)
            .WithMany()
            .HasForeignKey(x => x.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.Followee)
            .WithMany()
            .HasForeignKey(x => x.FolloweeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.Follower.IsDeleted && !x.Followee.IsDeleted);
    }
}
