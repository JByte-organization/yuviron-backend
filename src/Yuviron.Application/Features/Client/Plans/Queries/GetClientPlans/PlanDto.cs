using System;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Plans.Queries.GetClientPlans;

public sealed record PlanDto(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    PlanPeriod Period,
    PlanType Type
);