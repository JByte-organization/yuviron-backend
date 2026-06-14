using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Utilities;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

namespace Yuviron.Application.Features.Client.Search.Queries.SearchArtists;

public sealed class SearchArtistsHandler : IRequestHandler<SearchArtistsQuery, PaginatedList<SearchArtistDto>>
{
    private readonly ICatalogContext _catalogContext;

    public SearchArtistsHandler(ICatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    public async Task<PaginatedList<SearchArtistDto>> Handle(SearchArtistsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page;
        var pageSize = request.PageSize;
        var searchTerm = SearchQueryNormalizer.Normalize(request.SearchTerm);
        var exactQuery = _catalogContext.BuildPublicArtistSearchQuery(searchTerm);

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm))
        {
            return await exactQuery
                .ApplyArtistSearchOrdering()
                .SelectSearchArtistDtos()
                .ToPaginatedListAsync(page, pageSize, cancellationToken);
        }

        var exactCount = await exactQuery.CountAsync(cancellationToken);
        var requiredExactResults = (long)page * pageSize;

        if (exactCount >= requiredExactResults)
        {
            var items = await exactQuery
                .ApplyArtistSearchOrdering()
                .SelectSearchArtistDtos()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<SearchArtistDto>(items, exactCount, page, pageSize);
        }

        var exactRows = await exactQuery
            .ApplyArtistSearchOrdering()
            .Take(requiredExactResults > int.MaxValue ? int.MaxValue : (int)requiredExactResults)
            .SelectArtistSearchRows()
            .ToListAsync(cancellationToken);

        var exactIds = exactRows
            .Select(row => row.Id)
            .ToHashSet();

        var fragments = SearchFuzzyMatcher.BuildCandidateFragments(searchTerm);
        var fuzzyRows = await _catalogContext
            .BuildPublicArtistFuzzyCandidateQuery(fragments)
            .SelectArtistSearchRows()
            .ToListAsync(cancellationToken);

        var rankedFuzzyRows = fuzzyRows
            .Where(row => !exactIds.Contains(row.Id))
            .Select(row => SearchFuzzyMatcher.TryGetScore(searchTerm, row.GetSearchableTexts(), out var score)
                ? new { Row = row, Score = score }
                : null)
            .Where(match => match is not null)
            .Select(match => match!)
            .OrderByDescending(match => match.Score)
            .ThenByDescending(match => match.Row.TotalPlays)
            .ThenBy(match => match.Row.Name)
            .Select(match => match.Row)
            .ToList();

        var combinedItems = exactRows
            .Select(row => row.ToDto())
            .Concat(rankedFuzzyRows.Select(row => row.ToDto()))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<SearchArtistDto>(
            combinedItems,
            exactCount + rankedFuzzyRows.Count,
            page,
            pageSize);
    }
}
