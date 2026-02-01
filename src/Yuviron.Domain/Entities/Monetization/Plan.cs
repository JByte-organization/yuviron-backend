using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum PlanPeriod { Month = 1, Year = 2 }

public class Plan : Entity
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PlanPeriod Period { get; private set; }

    private Plan() { }

    public static Plan Create(string name, decimal price, string currency, PlanPeriod period)
    {
        return new Plan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Currency = currency,
            Period = period
        };
    }
}
