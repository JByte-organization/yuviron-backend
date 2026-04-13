using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class ReleaseNotificationTemplate : Entity
{
    public Guid ArtistId { get; private set; }
    public string TitleTemplate { get; private set; } = string.Empty;
    public string BodyTemplate { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }

    public virtual Artist Artist { get; private set; } = null!;

    private ReleaseNotificationTemplate() { }

    public static ReleaseNotificationTemplate Create(
        Guid artistId, 
        string titleTemplate, 
        string bodyTemplate, 
        bool isEnabled)
    {
        if (string.IsNullOrWhiteSpace(titleTemplate)) throw new ArgumentException("Title template is required");
        if (string.IsNullOrWhiteSpace(bodyTemplate)) throw new ArgumentException("Body template is required");

        return new ReleaseNotificationTemplate
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            TitleTemplate = titleTemplate.Trim(),
            BodyTemplate = bodyTemplate.Trim(),
            IsEnabled = isEnabled
        };
    }

    public void UpdateTemplate(string titleTemplate, string bodyTemplate)
    {
        if (string.IsNullOrWhiteSpace(titleTemplate)) throw new ArgumentException("Title template is required");
        if (string.IsNullOrWhiteSpace(bodyTemplate)) throw new ArgumentException("Body template is required");

        TitleTemplate = titleTemplate.Trim();
        BodyTemplate = bodyTemplate.Trim();
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
}