using MediatR;
using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;


public record SearchResultItemDto(
    Guid Id,
    string Title,
    string? Subtitle, // Для трека это будет имя артиста, для артиста - жанр и т.д.
    string? CoverUrl,
    string Type // "track", "artist", "playlist"
);

public record GlobalSearchResponse(
    List<SearchResultItemDto> Tracks,
    List<SearchResultItemDto> Artists,
    List<SearchResultItemDto> Playlists
);


public record GlobalSearchQuery(
    string Query,
    int Limit = 5 
) : IRequest<GlobalSearchResponse>;