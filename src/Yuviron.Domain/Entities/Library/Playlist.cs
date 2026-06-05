using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Domain.Entities;

public class Playlist : Entity
{
    public Guid? UserId { get; private set; } 
    
    // ДОБАВЛЕНО: Привязка к профилю артиста (если плейлист кураторский)
    public Guid? ArtistId { get; private set; } 
    
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CoverUrl { get; private set; }
    
    public PlaylistVisibility Visibility { get; private set; } 
    public bool IsEditorial { get; private set; } 
    public bool IsDeleted { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual User? User { get; private set; }
    
    public virtual Artist? Artist { get; private set; } 
    
    public virtual ICollection<PlaylistTrack> PlaylistTracks { get; private set; } = new List<PlaylistTrack>();

    private Playlist() { }

    public static Playlist Create(
        Guid? userId, 
        Guid? artistId,
        string title, 
        string? description, 
        string? coverUrl, 
        PlaylistVisibility visibility,
        bool isEditorial, 
        DateTime utcNow)
    {
        if (isEditorial)
        {
            userId = null;
            artistId = null;
        }
        
        return new Playlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArtistId = artistId, 
            Title = title.Trim(),
            Description = description?.Trim(),
            CoverUrl = coverUrl,
            Visibility = visibility,
            IsEditorial = isEditorial,
            IsDeleted = false,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void Update(
        string title, 
        string? description, 
        string? coverUrl, 
        PlaylistVisibility visibility,
        bool isEditorial, 
        Guid? userId,   
        Guid? artistId, 
        DateTime utcNow)
    {
        if (isEditorial)
        {
            userId = null;
            artistId = null;
        }

        Title = title.Trim();
        Description = description?.Trim();
        CoverUrl = coverUrl;
        Visibility = visibility;
        
        IsEditorial = isEditorial; 
        UserId = userId;    
        ArtistId = artistId; 
        
        UpdatedAt = utcNow;
    }
    
    public void NotifyContentChanged(DateTime utcNow)
    {
        UpdatedAt = utcNow;
    }

    public void Delete(DateTime utcNow)
    {
        IsDeleted = true;
        UpdatedAt = utcNow;

        if (!string.IsNullOrWhiteSpace(CoverUrl))
        {
            AddDomainEvent(new FileNeedsDeletionEvent(CoverUrl));
        }
    }
}