using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Commands.AddTeamMember;

public sealed record AddTeamMemberCommand(
    Guid ArtistId,
    Guid UserId,
    ArtistTeamRole Role
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}