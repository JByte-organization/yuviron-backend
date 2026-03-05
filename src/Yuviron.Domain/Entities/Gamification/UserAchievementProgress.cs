using System;

namespace Yuviron.Domain.Entities;

public class UserAchievementProgress
{
    public Guid UserId { get; private set; }
    public Guid AchievementId { get; private set; }
    public string MetricKey { get; private set; } = string.Empty; 
    public int MetricValue { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Achievement Achievement { get; private set; } = null!;

    private UserAchievementProgress() { }

    public static UserAchievementProgress Create(Guid userId, Guid achievementId, string metricKey, int initialValue, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(metricKey)) throw new ArgumentException("Metric key is required");
        if (initialValue < 0) throw new ArgumentException("Initial value cannot be negative");

        return new UserAchievementProgress
        {
            UserId = userId,
            AchievementId = achievementId,
            MetricKey = metricKey.Trim().ToLowerInvariant(),
            MetricValue = initialValue,
            UpdatedAt = utcNow
        };
    }

    // Удобный метод для обновления прогресса
    public void Increment(int amount, DateTime utcNow)
    {
        if (amount < 0) throw new ArgumentException("Cannot increment by a negative amount");
        
        MetricValue += amount;
        UpdatedAt = utcNow;
    }
    
    // На случай, если нужно задать точное значение прогресса
    public void SetValue(int newValue, DateTime utcNow)
    {
        if (newValue < 0) throw new ArgumentException("Value cannot be negative");

        MetricValue = newValue;
        UpdatedAt = utcNow;
    }
}