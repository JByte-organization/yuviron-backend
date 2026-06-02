using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;

public sealed record UpdateTrackCommand(
    Guid TrackId,
    string Title,
    bool Explicit,
    int? Position,
    Guid? CoverFileId, 
    List<Guid>? GenreIds,
    List<Guid>? MoodIds,
    List<StudioTrackCollaboratorInput>? Collaborators
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}