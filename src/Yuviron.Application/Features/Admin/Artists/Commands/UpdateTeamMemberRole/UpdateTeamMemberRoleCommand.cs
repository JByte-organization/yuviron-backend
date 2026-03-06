using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Commands.UpdateTeamMemberRole;

public sealed record UpdateTeamMemberRoleCommand(
    Guid ArtistId,
    Guid UserId,
    ArtistTeamRole NewRole
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}