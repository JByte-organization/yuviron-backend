using MediatR;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistTracks;

public sealed record GetPlaylistTracksQuery(
    Guid PlaylistId,
    int Page = 1,
    int PageSize = 50
) : IRequest<PaginatedList<PlaylistTrackItemClientDto>>;