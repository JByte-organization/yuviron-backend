using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Playlist : Entity
{
    public Guid OwnerUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CoverUrl { get; private set; }
    public bool IsPublic { get; private set; }
    public bool IsDeleted { get; private set; } // Soft Delete
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual ICollection<PlaylistTrack> PlaylistTracks { get; private set; } = new List<PlaylistTrack>();

    private Playlist() { }
}