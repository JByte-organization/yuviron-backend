using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class ArtistSocialLink : Entity
{
    public Guid ArtistId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public virtual Artist Artist { get; private set; } = null!;

    private ArtistSocialLink() { }

    public static ArtistSocialLink Create(Guid artistId, string type, string url)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required");
        return new ArtistSocialLink { Id = Guid.NewGuid(), ArtistId = artistId, Type = type, Url = url };
    }
}
