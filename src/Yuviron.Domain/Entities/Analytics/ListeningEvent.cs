using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public sealed class ListeningEvent : Entity
{
    public Guid? UserId { get; private set; } 
    public Guid TrackId { get; private set; }
    public DateTime PlayedAt { get; private set; }
    public int MsPlayed { get; private set; }

    public PlaybackDeviceType DeviceType { get; private set; } 
    public string? CountryCode { get; private set; }

    public PlaybackSourceType SourceType { get; private set; } 
    public Guid? SourceId { get; private set; }

    public User? User { get; private set; }
    public Track Track { get; private set; } = null!;

    private ListeningEvent() { }

    public static ListeningEvent Create(
        Guid? userId,
        Guid trackId,
        int msPlayed,
        PlaybackDeviceType deviceType, 
        string? countryCode,
        PlaybackSourceType sourceType, 
        Guid? sourceId,
        DateTime utcNow) 
    {
        if (trackId == Guid.Empty) throw new ArgumentException("TrackId cannot be empty");
        if (msPlayed < 0) throw new ArgumentException("Cannot play negative time");

        return new ListeningEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TrackId = trackId,
            PlayedAt = utcNow,
            MsPlayed = msPlayed,
            DeviceType = deviceType, 
            CountryCode = countryCode?.Trim().ToUpper(), 
            SourceType = sourceType,
            SourceId = sourceId
        };
    }
}