namespace Yuviron.Domain.Entities;

public class TrackListenHeatmap
{
    // Ключ: Трек + Секунда
    public Guid TrackId { get; private set; }
    public int SecondIndex { get; private set; }

    public int PlaysCount { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual Track Track { get; private set; } = null!;

    // Пустой конструктор для EF Core
    private TrackListenHeatmap() { }

    // Наш конструктор
    public TrackListenHeatmap(Guid trackId, int secondIndex)
    {
        if (secondIndex < 0) throw new ArgumentException("Seconds cannot be negative");

        TrackId = trackId;
        SecondIndex = secondIndex;
        PlaysCount = 1; // Создали - значит 1 раз уже послушали
        UpdatedAt = DateTime.UtcNow;
    }

    // Метод действия (Behavior)
    public void IncrementPlays()
    {
        PlaysCount++;
        UpdatedAt = DateTime.UtcNow;
    }
}