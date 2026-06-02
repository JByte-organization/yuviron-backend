using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;

public sealed record CreateStudioArtistTrackCommand(
    Guid AlbumId,
    string Title,
    bool Explicit,
    Guid AudioFileId,
    Guid? CoverFileId,
    List<Guid>? CoAuthorIds
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
