using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Plans.Commands.UpdatePlan;

public sealed record UpdatePlanCommand(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    PlanPeriod Period,
    PlanType Type 
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}