using System;
using System.Collections.Generic;
using System.Linq;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class Track : Entity
{
    public Guid? AlbumId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int DurationMs { get; private set; }
    public bool Explicit { get; private set; }
    public string? CoverUrl { get; private set; }
    public string AudioStorageKey { get; private set; } = string.Empty;
    public string? PreviewStorageKey { get; private set; }
    public VisibilityStatus VisibilityStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public virtual Album? Album { get; private set; }
    public virtual ICollection<TrackArtist> TrackArtists { get; private set; } = new List<TrackArtist>();
    public virtual ICollection<TrackGenre> TrackGenres { get; private set; } = new List<TrackGenre>();
    
    public virtual ICollection<TrackMood> TrackMoods { get; private set; } = new List<TrackMood>();

    private Track() { }

    public static Track Create(
        Guid? albumId, string title, int durationMs, bool isExplicit, string? coverUrl,
        string audioKey, string? previewKey, VisibilityStatus status,
        IEnumerable<Guid> artistIds, IEnumerable<Guid> genreIds, 
        IEnumerable<Guid> moodIds,
        DateTime utcNow)
    {
        if (durationMs <= 0) throw new ArgumentException("Duration must be positive");
        if (string.IsNullOrWhiteSpace(audioKey)) throw new ArgumentException("Audio key is required");

        var track = new Track
        {
            Id = Guid.NewGuid(),
            AlbumId = albumId,
            Title = title.Trim(),
            DurationMs = durationMs,
            Explicit = isExplicit,
            CoverUrl = coverUrl?.Trim(),
            AudioStorageKey = audioKey,
            PreviewStorageKey = previewKey,
            VisibilityStatus = status,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        track.SyncArtists(artistIds);
        track.SyncGenres(genreIds);
        track.SyncMoods(moodIds);
        return track;
    }

    public void UpdateDetails(
        Guid? albumId, string title, int durationMs, bool isExplicit, string? coverUrl,
        string audioKey, string? previewKey, VisibilityStatus status,
        IEnumerable<Guid> artistIds, IEnumerable<Guid> genreIds, 
        IEnumerable<Guid> moodIds,
        DateTime utcNow)
    {
        AlbumId = albumId;
        Title = title.Trim();
        DurationMs = durationMs;
        Explicit = isExplicit;
        CoverUrl = coverUrl?.Trim();
        AudioStorageKey = audioKey;
        PreviewStorageKey = previewKey;
        VisibilityStatus = status;
        UpdatedAt = utcNow;

        SyncArtists(artistIds);
        SyncGenres(genreIds);
        SyncMoods(moodIds);
    }

    private void SyncArtists(IEnumerable<Guid> ids)
{
    var newIds = ids.Distinct().ToList();
    
    var toRemove = TrackArtists.Where(ta => !newIds.Contains(ta.ArtistId)).ToList();
    foreach (var item in toRemove) TrackArtists.Remove(item);

    var currentIds = TrackArtists.Select(ta => ta.ArtistId).ToList();
    
    foreach (var id in newIds.Where(id => !currentIds.Contains(id)))
    {
        // Используем конструктор
        TrackArtists.Add(new TrackArtist(this.Id, id, ArtistRole.Main));
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

    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = utcNow;
        UpdatedAt = utcNow;
        AddDomainEvent(new TrackDeletedEvent(Id));
    }
}