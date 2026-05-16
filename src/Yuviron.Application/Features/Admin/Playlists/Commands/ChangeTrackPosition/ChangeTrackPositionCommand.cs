using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.ChangeTrackPosition;

public sealed record ChangeTrackPositionRequest(double NewPosition);

public sealed record ChangeTrackPositionCommand(
    Guid PlaylistId, 
    Guid TrackId, 
    double NewPosition
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}