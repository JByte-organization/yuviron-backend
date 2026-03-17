using Yuviron.Domain.Common;
using Yuviron.Domain.Enums; 

namespace Yuviron.Domain.Entities;

public class Playlist : Entity
{
    public Guid? UserId { get; private set; } 
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CoverUrl { get; private set; }
    
    public PlaylistVisibility Visibility { get; private set; } 
    public bool IsEditorial { get; private set; } 
    public bool IsDeleted { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual User? User { get; private set; }
    public virtual ICollection<PlaylistTrack> PlaylistTracks { get; private set; } = new List<PlaylistTrack>();

    private Playlist() { }

    public static Playlist Create(
        Guid? userId, 
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
        }
        
        return new Playlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
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
        DateTime utcNow)
    {
        Title = title.Trim();
        Description = description?.Trim();
        CoverUrl = coverUrl;
        Visibility = visibility;
        UpdatedAt = utcNow;
    }

    public void Delete(DateTime utcNow)
    {
        IsDeleted = true;
        UpdatedAt = utcNow;
    }
}