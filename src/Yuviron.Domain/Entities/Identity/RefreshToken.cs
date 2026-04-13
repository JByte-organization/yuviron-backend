using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public sealed class RefreshToken : Entity 
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt, DateTime utcNow)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = token,
            ExpiresAt = expiresAt,
            CreatedAt = utcNow 
        };
    }

    public void Revoke(DateTime utcNow) 
    {
        if (RevokedAt != null) return;
        RevokedAt = utcNow;
    }

    public bool IsRevoked => RevokedAt.HasValue;
    
    public bool IsExpired(DateTime utcNow) => ExpiresAt < utcNow;
    
    public bool IsInGracePeriod(DateTime utcNow) 
    {
        return IsRevoked && utcNow <= RevokedAt!.Value.AddMinutes(1);
    }
}