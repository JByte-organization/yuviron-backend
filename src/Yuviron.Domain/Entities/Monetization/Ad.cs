using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Ad : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string MediaUrl { get; private set; } = string.Empty; // Аудио или баннер
    public bool IsActive { get; private set; }

    public virtual ICollection<AdImpression> Impressions { get; private set; } = new List<AdImpression>();

    private Ad() { }
}
