
namespace Yuviron.Domain.Entities;

public class TrackMood
{
    public Guid TrackId { get; private set; }
    public virtual Track Track { get; private set; } = null!;

    public Guid MoodId { get; private set; }
    public virtual Mood Mood { get; private set; } = null!;

    private TrackMood() { } 

    public TrackMood(Guid trackId, Guid moodId)
    {
        TrackId = trackId;
        MoodId = moodId;
    }
}