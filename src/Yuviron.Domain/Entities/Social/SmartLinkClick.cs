using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class SmartLinkClick : Entity
{
    public Guid SmartLinkId { get; private set; }
    public DateTime ClickedAt { get; private set; }
    public string? CountryCode { get; private set; }
    public string? Referrer { get; private set; }
    public string? DeviceType { get; private set; }

    public virtual SmartLink SmartLink { get; private set; } = null!;

    private SmartLinkClick() { }

    public static SmartLinkClick Create(Guid smartLinkId, string? countryCode, string? referrer, string? deviceType, DateTime utcNow)
    {
        return new SmartLinkClick
        {
            Id = Guid.NewGuid(),
            SmartLinkId = smartLinkId,
            ClickedAt = utcNow,
            CountryCode = countryCode?.Trim().ToUpper(),
            Referrer = referrer?.Trim(),
            DeviceType = string.IsNullOrWhiteSpace(deviceType) ? "unknown" : deviceType.Trim()
        };
    }
}