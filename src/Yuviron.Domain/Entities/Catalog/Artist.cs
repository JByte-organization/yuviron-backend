using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class Artist : Entity
{
    public Guid? OwnerUserId { get; private set; } // Если артистом управляет юзер
    public string Name { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public bool IsVerified { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual User? OwnerUser { get; private set; }

    public virtual ICollection<ArtistSocialLink> SocialLinks { get; private set; } = new List<ArtistSocialLink>();
    public virtual ICollection<ArtistPin> Pins { get; private set; } = new List<ArtistPin>();

    // Связи с музыкой
    public virtual ICollection<AlbumArtist> AlbumArtists { get; private set; } = new List<AlbumArtist>();
    public virtual ICollection<TrackArtist> TrackArtists { get; private set; } = new List<TrackArtist>();

    private Artist() { }
}
