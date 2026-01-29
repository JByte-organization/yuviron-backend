using System;
using System.Collections.Generic;
using System.Text;
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
}
