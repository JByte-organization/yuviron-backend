using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public enum ArtistRole { Main = 1, Feat = 2, Producer = 3 }

public class AlbumArtist
{
    public Guid AlbumId { get; private set; }
    public virtual Album Album { get; set; } = null!;

    public Guid ArtistId { get; private set; }
    public virtual Artist Artist { get; set; } = null!;

    public ArtistRole Role { get; private set; }
    
    private AlbumArtist() { }

    public AlbumArtist(Guid albumId, Guid artistId, ArtistRole role)
    {
        AlbumId = albumId;
        ArtistId = artistId;
        Role = role;
    }
}
