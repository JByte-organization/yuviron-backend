using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbumTracks;

public sealed record GetStudioAlbumTracksQuery(Guid AlbumId) : IRequest<List<StudioAlbumTrackDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}