using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class Track : Entity
{
    public Guid AlbumId { get; private set; } 
    public int AlbumPosition { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Isrc { get; private set; }
    public int DurationMs { get; private set; }
    public bool Explicit { get; private set; }
    public string? CoverUrl { get; private set; }
    public string AudioStorageKey { get; private set; } = string.Empty;
    
    public long PlayCount { get; private set; }
    public VisibilityStatus VisibilityStatus { get; private set; }
    
    public TrackProcessingStatus ProcessingStatus { get; private set; }
    public string? HlsPlaylistUrl { get; private set; } 
    
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public virtual Album? Album { get; private set; }
    public virtual Lyrics? Lyrics { get; private set; }
    public virtual ICollection<TrackArtist> TrackArtists { get; private set; } = new List<TrackArtist>();
    public virtual ICollection<TrackGenre> TrackGenres { get; private set; } = new List<TrackGenre>();
    
    public virtual ICollection<TrackMood> TrackMoods { get; private set; } = new List<TrackMood>();

    private Track() { }

    public static Track Create(
        Guid id, Guid albumId, int albumPosition, string title, int durationMs, bool isExplicit, string? coverUrl,
        string audioKey, VisibilityStatus status,
        string? isrc,
        IEnumerable<(Guid ArtistId, ArtistRole Role)> artists,
        IEnumerable<Guid> genreIds, 
        IEnumerable<Guid> moodIds,
        DateTime utcNow)
    {
        if (albumId == Guid.Empty) throw new ArgumentException("Album is required");
        if (albumPosition <= 0) throw new ArgumentException("Album position must be positive");
        if (durationMs <= 0) throw new ArgumentException("Duration must be positive");
        if (string.IsNullOrWhiteSpace(audioKey)) throw new ArgumentException("Audio key is required");

        var track = new Track
        {
            Id = id,
            AlbumId = albumId,
            AlbumPosition = albumPosition, 
            Title = title.Trim(),
            Isrc = isrc?.Trim(),
            DurationMs = durationMs,
            Explicit = isExplicit,
            CoverUrl = coverUrl?.Trim(),
            AudioStorageKey = audioKey,
            VisibilityStatus = status,
            
            ProcessingStatus = TrackProcessingStatus.Processing, 
            
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        track.SyncArtists(artists);
        track.SyncGenres(genreIds);
        track.SyncMoods(moodIds);
        
        track.AddDomainEvent(new AudioNeedsTranscodingEvent(track.Id, audioKey));
        
        return track;
    }

    public void MarkAsReady(string hlsPlaylistUrl, string finalAudioKey, DateTime utcNow)
    {
        HlsPlaylistUrl = hlsPlaylistUrl;
        AudioStorageKey = finalAudioKey; 
    
        ProcessingStatus = TrackProcessingStatus.Ready;
        UpdatedAt = utcNow;
    }

    public void MarkAsFailed(DateTime utcNow, string? cleanupDirectoryPath = null)
    {
        ProcessingStatus = TrackProcessingStatus.Failed;
        UpdatedAt = utcNow;

        if (!string.IsNullOrWhiteSpace(cleanupDirectoryPath))
        {
            AddDomainEvent(new DirectoryNeedsDeletionEvent(cleanupDirectoryPath));
        }
    }

    public void UpdateDetails(
        Guid albumId, int albumPosition, string title, int durationMs, bool isExplicit, string? coverUrl, 
        string audioKey, VisibilityStatus status,
        string? isrc,
        IEnumerable<(Guid ArtistId, ArtistRole Role)> artists,
        IEnumerable<Guid> genreIds, 
        IEnumerable<Guid> moodIds,
        DateTime utcNow, bool audioChanged = false)
    {
        AlbumId = albumId;
        AlbumPosition = albumPosition;
        Title = title.Trim();
        Isrc = isrc?.Trim();
        DurationMs = durationMs;
        Explicit = isExplicit;
        CoverUrl = coverUrl?.Trim();
        AudioStorageKey = audioKey;
        VisibilityStatus = status;
        UpdatedAt = utcNow;

        if (audioChanged)
        {
            ProcessingStatus = TrackProcessingStatus.Processing;
            HlsPlaylistUrl = null;
        }

        SyncArtists(artists); 
        SyncGenres(genreIds);
        SyncMoods(moodIds);
    }

    private void SyncArtists(IEnumerable<(Guid ArtistId, ArtistRole Role)> artists)
    {
        var newArtists = artists.DistinctBy(a => a.ArtistId).ToList();
        var newIds = newArtists.Select(a => a.ArtistId).ToList();
        
        var toRemove = TrackArtists.Where(ta => !newIds.Contains(ta.ArtistId)).ToList();
        foreach (var item in toRemove) TrackArtists.Remove(item);

        foreach (var newArtist in newArtists)
        {
            var existingArtist = TrackArtists.FirstOrDefault(ta => ta.ArtistId == newArtist.ArtistId);
            
            if (existingArtist == null)
            {
                TrackArtists.Add(new TrackArtist(this.Id, newArtist.ArtistId, newArtist.Role));
            }
            else if (existingArtist.Role != newArtist.Role)
            {
                existingArtist.UpdateRole(newArtist.Role);
            }
        }
    }
    
    public void SetLyrics(string? text, string languageCode = "en")
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Lyrics = null;
            return;
        }

        if (Lyrics == null)
        {
            Lyrics = Lyrics.Create(this.Id, languageCode, text);
        }
        else
        {
            Lyrics.UpdateText(text);
        }
    }

    private void SyncGenres(IEnumerable<Guid> ids)
    {
        var newIds = ids.Distinct().ToList();
        var toRemove = TrackGenres.Where(tg => !newIds.Contains(tg.GenreId)).ToList();
        foreach (var item in toRemove) TrackGenres.Remove(item);

        var currentIds = TrackGenres.Select(tg => tg.GenreId).ToList();
        foreach (var id in newIds.Where(id => !currentIds.Contains(id)))
        {
            TrackGenres.Add(new TrackGenre(Id, id)); 
        }
    }

    private void SyncMoods(IEnumerable<Guid> ids)
    {
        var newIds = ids.Distinct().ToList();
        var toRemove = TrackMoods.Where(tm => !newIds.Contains(tm.MoodId)).ToList();
        foreach (var item in toRemove) TrackMoods.Remove(item);

        var currentIds = TrackMoods.Select(tm => tm.MoodId).ToList();
        foreach (var id in newIds.Where(id => !currentIds.Contains(id)))
        {
            TrackMoods.Add(new TrackMood(Id, id));
        }
    }
    
    public void AddPlays(long count)
    {
        if (count > 0) 
        {
            PlayCount += count;
        }
    }

    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = utcNow;
        UpdatedAt = utcNow;

        AddDomainEvent(new TrackDeletedEvent(Id));
        
        if (!string.IsNullOrWhiteSpace(CoverUrl))
        {
            AddDomainEvent(new FileNeedsDeletionEvent(CoverUrl));
        }

        if (!string.IsNullOrWhiteSpace(AudioStorageKey))
        {
            AddDomainEvent(new FileNeedsDeletionEvent(AudioStorageKey));
        }

        AddDomainEvent(new DirectoryNeedsDeletionEvent($"tracks/{Id}"));
    }
}
