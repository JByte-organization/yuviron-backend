using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class Track : Entity
{
    public Guid? AlbumId { get; private set; } // Nullable (Сингл)

    public string Title { get; private set; } = string.Empty;
    public int DurationMs { get; private set; }
    public bool Explicit { get; private set; }

    public string? CoverUrl { get; private set; } // Если отличается от альбома
    public string AudioStorageKey { get; private set; } = string.Empty; // Путь к S3
    public string? PreviewStorageKey { get; private set; }

    public VisibilityStatus VisibilityStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual Album? Album { get; private set; }

    public virtual ICollection<TrackArtist> TrackArtists { get; private set; } = new List<TrackArtist>();
    public virtual ICollection<TrackGenre> TrackGenres { get; private set; } = new List<TrackGenre>();

    private Track() { }
}
