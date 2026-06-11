using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class ArtistSocialLink : Entity
{
    public Guid ArtistId { get; private set; }
    public SocialLinkType Type { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    
    public virtual Artist Artist { get; private set; } = null!;

    private ArtistSocialLink() { }

    public static ArtistSocialLink Create(Guid artistId, SocialLinkType type, string url, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required");
        
        return new ArtistSocialLink
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            Type = type,
            Url = url.Trim(),
            CreatedAt = utcNow
        };
    }
}