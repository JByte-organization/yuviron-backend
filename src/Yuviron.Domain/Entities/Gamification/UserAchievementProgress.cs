using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class UserAchievementProgress
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid AchievementId { get; set; }
    public virtual Achievement Achievement { get; set; } = null!;

    public string MetricKey { get; set; } = string.Empty; // "tracks_listened"
    public int MetricValue { get; set; }
    public DateTime UpdatedAt { get; set; }
}
