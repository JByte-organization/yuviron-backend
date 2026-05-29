using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Plans.Commands.DeletePlan;

public sealed record DeletePlanCommand(Guid Id) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}