using System;
using System.Collections.Generic;
using System.Text;
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
}
