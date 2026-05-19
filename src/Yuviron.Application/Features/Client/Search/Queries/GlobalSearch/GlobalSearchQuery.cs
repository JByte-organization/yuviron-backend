using MediatR;
using System;
using System.Collections.Generic;
using Yuviron.Application.Features.Client.Search.Queries.SearchGenres;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public sealed record GlobalSearchResponse(
    List<SearchTrackDto> Tracks,
    List<SearchArtistDto> Artists,
    List<SearchAlbumDto> Albums,
    List<SearchPlaylistDto> Playlists,
    List<SearchGenreMoodDto> Genres,
    int Total
);

public sealed record GlobalSearchQuery(
    string? Query,
    int Limit = 5
) : IRequest<GlobalSearchResponse>;
