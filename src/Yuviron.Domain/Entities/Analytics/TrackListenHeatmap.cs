using System;

namespace Yuviron.Domain.Entities;

public class TrackListenHeatmap
{
    // Композитный ключ: TrackId + SecondIndex (настраивается в EF Core Configuration)
    public Guid TrackId { get; private set; }
    public int SecondIndex { get; private set; }

    public int PlaysCount { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual Track Track { get; private set; } = null!;

    private TrackListenHeatmap() { }

    public static TrackListenHeatmap Create(Guid trackId, int secondIndex, DateTime utcNow)
    {
        if (secondIndex < 0) throw new ArgumentException("Seconds cannot be negative");

        return new TrackListenHeatmap
        {
            TrackId = trackId,
            SecondIndex = secondIndex,
            PlaysCount = 1, // Создали - значит 1 раз уже послушали
            UpdatedAt = utcNow
        };
    }

    // Метод действия с пробросом времени
    public void IncrementPlays(DateTime utcNow)
    {
        PlaysCount++;
        UpdatedAt = utcNow;
    }
}