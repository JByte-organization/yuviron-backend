using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;

public sealed record CreateTrackCommand(
    Guid AlbumId,
    string Title,
    Guid AudioFileId, 
    Guid? CoverFileId, 
    bool Explicit,
    int? Position, 
    List<Guid>? GenreIds,
    List<Guid>? MoodIds,
    List<StudioTrackCollaboratorInput>? Collaborators
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}