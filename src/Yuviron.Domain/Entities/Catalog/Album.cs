using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
namespace Yuviron.Domain.Entities;

public class Album : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CoverUrl { get; private set; }
    public ReleaseType ReleaseType { get; private set; }
    public DateTime ReleaseDate { get; private set; }
    public VisibilityStatus VisibilityStatus { get; private set; }
    public DateTime? ScheduledPublishAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public virtual ICollection<Track> Tracks { get; private set; } = new List<Track>();
    public virtual ICollection<AlbumArtist> AlbumArtists { get; private set; } = new List<AlbumArtist>();

    private Album() { }

    public static Album Create(
        string title, string? description, string? coverUrl, DateTime releaseDate,
        ReleaseType releaseType,
        VisibilityStatus visibilityStatus, DateTime? scheduledPublishAt,
        IEnumerable<Guid> artistIds, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");

        var album = new Album
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim(),
            CoverUrl = coverUrl?.Trim(),
            ReleaseType = releaseType,
            ReleaseDate = releaseDate,
            VisibilityStatus = visibilityStatus,
            ScheduledPublishAt = NormalizeScheduledPublishAt(visibilityStatus, scheduledPublishAt),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        album.SyncArtists(artistIds);
        return album;
    }

    public void UpdateDetails(
        string title, string? description, string? newCoverUrl, DateTime releaseDate,
        ReleaseType releaseType,
        VisibilityStatus visibilityStatus, DateTime? scheduledPublishAt,
        IEnumerable<Guid> artistIds, DateTime utcNow)
    {
        Title = title.Trim();
        Description = description?.Trim();
        ReleaseType = releaseType;
        ReleaseDate = releaseDate;
        VisibilityStatus = visibilityStatus;
        ScheduledPublishAt = NormalizeScheduledPublishAt(visibilityStatus, scheduledPublishAt);
        UpdatedAt = utcNow;
        
        SyncArtists(artistIds);
    }

    private void SyncArtists(IEnumerable<Guid> artistIds)
    {
        var newIds = artistIds.Distinct().ToList();
        
        var toRemove = AlbumArtists.Where(aa => !newIds.Contains(aa.ArtistId)).ToList();
        foreach (var item in toRemove) AlbumArtists.Remove(item);

        var currentIds = AlbumArtists.Select(aa => aa.ArtistId).ToList();
        foreach (var id in newIds.Where(id => !currentIds.Contains(id)))
        {
            AlbumArtists.Add(new AlbumArtist(Id, id, ArtistRole.Main));
        }
    }

    public void Publish(DateTime utcNow)
    {
        if (VisibilityStatus == VisibilityStatus.Published) return;
        VisibilityStatus = VisibilityStatus.Published;
        ScheduledPublishAt = null;
        UpdatedAt = utcNow;
    }

    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = utcNow;
        UpdatedAt = utcNow;
        
        AddDomainEvent(new AlbumDeletedEvent(Id));

        if (!string.IsNullOrWhiteSpace(CoverUrl))
        {
            AddDomainEvent(new FileNeedsDeletionEvent(CoverUrl));
        }
    }

    private static DateTime? NormalizeScheduledPublishAt(VisibilityStatus status, DateTime? date)
    {
        if (status == VisibilityStatus.Scheduled)
        {
            if (!date.HasValue)
            {
                throw new InvalidOperationException("ScheduledPublishAt date is required when VisibilityStatus is Scheduled.");
            }
            return date; 
        }

        return null; 
    }
}

