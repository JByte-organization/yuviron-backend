using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;

public sealed record DeleteStudioArtistTrackCommand(Guid TrackId) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
