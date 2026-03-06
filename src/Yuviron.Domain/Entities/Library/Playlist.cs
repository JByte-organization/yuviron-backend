using Yuviron.Domain.Common;
namespace Yuviron.Domain.Entities;
public class Playlist : Entity
{
    public Guid? UserId { get; private set; } // Null если это редакционный плейлист
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CoverUrl { get; private set; }
    public bool IsPublic { get; private set; }
    public bool IsEditorial { get; private set; } // Добавили!
    public bool IsDeleted { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual User? User { get; private set; }
    public virtual ICollection<PlaylistTrack> PlaylistTracks { get; private set; } = new List<PlaylistTrack>();

    private Playlist() { }

    public static Playlist Create(Guid? userId, string title, string? description, string? coverUrl, bool isPublic, bool isEditorial, DateTime utcNow)
    {
        return new Playlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            Description = description?.Trim(),
            CoverUrl = coverUrl,
            IsPublic = isPublic,
            IsEditorial = isEditorial,
            IsDeleted = false,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
    
    public void SyncTracks(IEnumerable<(Guid TrackId, int Position)> tracks, Guid actorUserId, DateTime utcNow)
    {
        var trackList = tracks.ToList();
        var newTrackIds = trackList.Select(t => t.TrackId).ToList();

        // 1. Удаляем треки, которых нет в новом списке
        var toRemove = PlaylistTracks.Where(pt => !newTrackIds.Contains(pt.TrackId)).ToList();
        foreach (var item in toRemove) PlaylistTracks.Remove(item);

        // 2. Обновляем существующие и добавляем новые
        foreach (var (trackId, position) in trackList)
        {
            var existing = PlaylistTracks.FirstOrDefault(pt => pt.TrackId == trackId);
            if (existing != null)
            {
                // Если трек уже был, просто обновляем его позицию
                existing.UpdatePosition(position);
            }
            else
            {
                // Если трека не было, создаем новую связь
                PlaylistTracks.Add(PlaylistTrack.Create(Id, trackId, position, actorUserId, utcNow));
            }
        }
        
        UpdatedAt = utcNow;
    }

    public void Update(string title, string? description, string? coverUrl, bool isPublic, DateTime utcNow)
    {
        Title = title.Trim();
        Description = description?.Trim();
        CoverUrl = coverUrl;
        IsPublic = isPublic;
        UpdatedAt = utcNow;
    }

    public void Delete(DateTime utcNow)
    {
        IsDeleted = true;
        UpdatedAt = utcNow;
    }
}