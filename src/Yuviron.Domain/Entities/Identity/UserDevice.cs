using Yuviron.Domain.Common;
using System;

namespace Yuviron.Domain.Entities;

public class UserDevice : Entity
{
    public Guid UserId { get; private set; }
    
    public string Fingerprint { get; private set; } = string.Empty; // <-- НОВАЯ КОЛОНКА
    
    public string DeviceName { get; private set; } = string.Empty; 
    public string BrowserName { get; private set; } = string.Empty; 
    public string LastIpAddress { get; private set; } = string.Empty; 
    
    public DateTime CreatedAt { get; private set; }
    public DateTime LastUsedAt { get; private set; }

    public virtual User User { get; private set; } = null!;

    private UserDevice() { }

    public static UserDevice Create(Guid userId, string fingerprint, string deviceName, string browserName, string ipAddress, DateTime utcNow)
    {
        return new UserDevice
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Fingerprint = fingerprint, 
            DeviceName = deviceName,
            BrowserName = browserName,
            LastIpAddress = ipAddress,
            CreatedAt = utcNow,
            LastUsedAt = utcNow
        };
    }

    public void UpdateLastUsed(string ipAddress, DateTime utcNow)
    {
        LastIpAddress = ipAddress;
        LastUsedAt = utcNow;
    }
}