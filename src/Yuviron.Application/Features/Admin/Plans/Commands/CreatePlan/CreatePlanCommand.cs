using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Plans.Commands.CreatePlan;

public sealed record CreatePlanCommand(
    string Name,
    decimal Price,
    string Currency,
    PlanPeriod Period,
    PlanType Type 
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}