namespace Yuviron.Domain.Entities;

public class UserAchievement
{
    public Guid UserId { get; private set; }
    public Guid AchievementId { get; private set; }
    public DateTime UnlockedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Achievement Achievement { get; private set; } = null!;

    private UserAchievement() { }

    public UserAchievement(Guid userId, Guid achievementId, DateTime utcNow)
    {
        UserId = userId;
        AchievementId = achievementId;
        UnlockedAt = utcNow;
    }
}