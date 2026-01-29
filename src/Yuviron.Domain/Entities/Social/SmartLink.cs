using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum SmartLinkType { Track = 1, Album = 2, Artist = 3 }

public class SmartLink : Entity
{
    public string Code { get; private set; } = string.Empty; // Короткий URL slug

    // Полиморфная цель
    public SmartLinkType EntityType { get; private set; }
    public Guid EntityId { get; private set; }

    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public virtual User CreatedByUser { get; private set; } = null!;
    public virtual ICollection<SmartLinkClick> Clicks { get; private set; } = new List<SmartLinkClick>();

    private SmartLink() { }
}