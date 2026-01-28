using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class RoyaltyAccrualDaily
{
    public Guid ArtistId { get; set; }
    public DateOnly Date { get; set; } // DateOnly удобно для EF Core 8+

    public int StreamsCount { get; set; }
    public decimal GrossAmount { get; set; } // Грязными
    public decimal PlatformFeeAmount { get; set; }
    public decimal NetAmount { get; set; } // Чистыми артисту

    public virtual Artist Artist { get; set; } = null!;
}
