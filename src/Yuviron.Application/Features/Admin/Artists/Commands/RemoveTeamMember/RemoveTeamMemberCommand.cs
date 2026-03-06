using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Commands.RemoveTeamMember;

public sealed record RemoveTeamMemberCommand(
    Guid ArtistId,
    Guid UserId
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}