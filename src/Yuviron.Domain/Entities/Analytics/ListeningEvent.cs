using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class ListeningEvent : Entity
{
    public Guid? UserId { get; private set; } // Null если гость
    public Guid TrackId { get; private set; }
    public DateTime PlayedAt { get; private set; }
    public int MsPlayed { get; private set; }

    public string DeviceType { get; private set; } = "unknown";
    public string? CountryCode { get; private set; }

    public string? SourceType { get; private set; } // Playlist, Album
    public Guid? SourceId { get; private set; }

    public virtual User? User { get; private set; }
    public virtual Track Track { get; private set; } = null!;

    private ListeningEvent() { }

    public static ListeningEvent Create(
        Guid? userId,
        Guid trackId,
        int msPlayed,
        string? deviceType,
        string? countryCode,
        string? sourceType,
        Guid? sourceId,
        DateTime utcNow) // Добавили проброс времени!
    {
        if (msPlayed < 0) throw new ArgumentException("Cannot play negative time");

        return new ListeningEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TrackId = trackId,
            PlayedAt = utcNow, // Пишем реальное время события
            MsPlayed = msPlayed,
            DeviceType = string.IsNullOrWhiteSpace(deviceType) ? "unknown" : deviceType.Trim(),
            CountryCode = countryCode?.Trim().ToUpper(), // Нормализуем код страны (например, "US")
            SourceType = sourceType?.Trim(),
            SourceId = sourceId
        };
    }
}