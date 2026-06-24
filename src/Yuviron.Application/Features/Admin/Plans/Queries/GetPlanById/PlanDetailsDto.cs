using System;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Plans.Queries.GetPlanById;

public sealed record PlanDetailsDto(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    PlanPeriod Period,
    string PlanTypeString,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
