using MediatR;
using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public record GlobalSearchResponse(
    List<SearchTrackDto> Tracks,
    List<SearchArtistDto> Artists,
    List<SearchPlaylistDto> Playlists
);

public record GlobalSearchQuery(
    string Query,
    int Limit = 5 
) : IRequest<GlobalSearchResponse>;