using MediatR;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistPlaylists;

public sealed record GetArtistPlaylistsQuery(
    Guid ArtistId,
    int Page = 1,
    int PageSize = 10
) : PaginatedQuery(null, Page, PageSize),
    IRequest<PaginatedList<ArtistPlaylistDto>>;
