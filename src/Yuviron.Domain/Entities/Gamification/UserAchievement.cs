using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class UserAchievement
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid AchievementId { get; set; }
    public virtual Achievement Achievement { get; set; } = null!;

    public DateTime UnlockedAt { get; set; }
}
