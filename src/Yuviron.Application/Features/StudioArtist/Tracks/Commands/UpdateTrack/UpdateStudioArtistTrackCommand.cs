using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;

public sealed record UpdateStudioArtistTrackCommand(
    Guid TrackId,
    string Title,
    Guid? CoverFileId,
    List<Guid>? CoAuthorIds
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
