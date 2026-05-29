using System;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Plans.Queries.GetPlans;

public sealed record PlanListItemDto(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    PlanPeriod Period,
    int ActiveSubscribersCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);