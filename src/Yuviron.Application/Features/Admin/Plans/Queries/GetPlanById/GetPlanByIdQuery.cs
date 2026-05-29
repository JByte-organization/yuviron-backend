using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Plans.Queries.GetPlanById;

public sealed record GetPlanByIdQuery(Guid Id) : IRequest<PlanDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}