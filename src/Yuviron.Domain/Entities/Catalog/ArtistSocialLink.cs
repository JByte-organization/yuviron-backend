using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class ArtistSocialLink : Entity
{
    public Guid ArtistId { get; private set; }
    public string Type { get; private set; } = string.Empty; // Instagram, VK...
    public string Url { get; private set; } = string.Empty;

    public virtual Artist Artist { get; private set; } = null!;

    private ArtistSocialLink() { }
}
