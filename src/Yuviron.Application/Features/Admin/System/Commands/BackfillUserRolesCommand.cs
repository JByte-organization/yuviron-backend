using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.System.Commands;

public sealed record BackfillUserRolesCommand : IRequest<Unit>, ISecuredRequest
{
    AppPermission ISecuredRequest.RequiredPermission => AppPermission.AccessAdminPanel;
}

