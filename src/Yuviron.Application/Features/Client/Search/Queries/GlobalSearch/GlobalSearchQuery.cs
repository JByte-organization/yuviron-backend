using MediatR;
using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;


public record SearchResultItemDto(
    Guid Id,
    string Title,
    string? Subtitle,
    string? CoverUrl,
    string Type
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