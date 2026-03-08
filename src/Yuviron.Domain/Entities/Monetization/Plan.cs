using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public sealed class Plan : Entity 
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PlanPeriod Period { get; private set; }

    private Plan() { }

    public static Plan Create(string name, decimal price, string currency, PlanPeriod period)
    {
        if (price < 0) throw new ArgumentException("Price cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.");

        return new Plan
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Price = price,
            Currency = currency.Trim().ToUpper(),
            Period = period
        };
    }
}