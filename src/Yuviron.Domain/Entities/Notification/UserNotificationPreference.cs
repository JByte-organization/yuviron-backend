using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class UserNotificationPreference : Entity
{
    public Guid UserId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public bool Enabled { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;

    private UserNotificationPreference() { }

    public static UserNotificationPreference Create(
        Guid userId,
        NotificationCategory category,
        string code,
        bool enabled,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Preference code is required.", nameof(code));

        return new UserNotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Category = category,
            Code = code.Trim(),
            Enabled = enabled,
            UpdatedAt = utcNow
        };
    }

    public void Update(bool enabled, DateTime utcNow)
    {
        Enabled = enabled;
        UpdatedAt = utcNow;
    }
}
