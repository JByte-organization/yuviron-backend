using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public enum ArtistRole { Main = 1, Feat = 2, Producer = 3 }

public class AlbumArtist
{
    public Guid AlbumId { get; set; }
    public virtual Album Album { get; set; } = null!;

    public Guid ArtistId { get; set; }
    public virtual Artist Artist { get; set; } = null!;

    public ArtistRole Role { get; set; }
}
