using System;
using System.Collections.Generic;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums; // Создай SmartLinkType в папке Enums

namespace Yuviron.Domain.Entities;

public class SmartLink : Entity
{
    public string Code { get; private set; } = string.Empty; 
    public SmartLinkType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public virtual User CreatedByUser { get; private set; } = null!;
    public virtual ICollection<SmartLinkClick> Clicks { get; private set; } = new List<SmartLinkClick>();

    private SmartLink() { }

    public static SmartLink Create(
        string code, 
        SmartLinkType entityType, 
        Guid entityId, 
        Guid createdByUserId, 
        DateTime? expiresAt, 
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required");

        return new SmartLink
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToLowerInvariant(),
            EntityType = entityType,
            EntityId = entityId,
            CreatedByUserId = createdByUserId,
            ExpiresAt = expiresAt,
            CreatedAt = utcNow
        };
    }

    public bool IsExpired(DateTime utcNow)
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < utcNow;
    }

    public void RecordClick(string? countryCode, string? referrer, string? deviceType, DateTime utcNow)
    {
        if (IsExpired(utcNow)) throw new InvalidOperationException("Cannot record click for an expired link");

        Clicks.Add(SmartLinkClick.Create(Id, countryCode, referrer, deviceType, utcNow));
    }
}