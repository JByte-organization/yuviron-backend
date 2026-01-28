using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class ArtistPayoutSettings
{
    public Guid ArtistId { get; set; } // PK и FK (1:1)

    public decimal MinWithdrawAmount { get; set; }
    public decimal MaxWithdrawAmount { get; set; }
    public decimal? CustomRatePerStream { get; set; } // Если у артиста спец. условия
    public int PlatformPercent { get; set; } // Комиссия платформы (например, 30%)

    public virtual Artist Artist { get; set; } = null!;
}
