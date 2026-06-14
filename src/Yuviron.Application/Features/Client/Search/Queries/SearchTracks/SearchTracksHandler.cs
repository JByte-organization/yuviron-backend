using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Utilities;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

namespace Yuviron.Application.Features.Client.Search.Queries.SearchTracks;

public sealed class SearchTracksHandler : IRequestHandler<SearchTracksQuery, PaginatedList<SearchTrackDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;

    public SearchTracksHandler(ICatalogContext catalogContext, TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<SearchTrackDto>> Handle(SearchTracksQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page;
        var pageSize = request.PageSize;
        var searchTerm = SearchQueryNormalizer.Normalize(request.SearchTerm);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var exactQuery = _catalogContext.BuildPublicTrackSearchQuery(searchTerm, utcNow);

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm))
        {
            return await exactQuery
                .ApplyTrackSearchOrdering()
                .SelectSearchTrackDtos()
                .ToPaginatedListAsync(page, pageSize, cancellationToken);
        }

        var exactCount = await exactQuery.CountAsync(cancellationToken);
        var requiredExactResults = (long)page * pageSize;

        if (exactCount >= requiredExactResults)
        {
            var items = await exactQuery
                .ApplyTrackSearchOrdering()
                .SelectSearchTrackDtos()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<SearchTrackDto>(items, exactCount, page, pageSize);
        }

        var exactRows = await exactQuery
            .ApplyTrackSearchOrdering()
            .Take(requiredExactResults > int.MaxValue ? int.MaxValue : (int)requiredExactResults)
            .SelectTrackSearchRows()
            .ToListAsync(cancellationToken);

        var exactIds = exactRows
            .Select(row => row.Id)
            .ToHashSet();

        var fragments = SearchFuzzyMatcher.BuildCandidateFragments(searchTerm);
        var fuzzyRows = await _catalogContext
            .BuildPublicTrackFuzzyCandidateQuery(fragments, utcNow)
            .SelectTrackSearchRows()
            .ToListAsync(cancellationToken);

        var rankedFuzzyRows = fuzzyRows
            .Where(row => !exactIds.Contains(row.Id))
            .Select(row => SearchFuzzyMatcher.TryGetScore(searchTerm, row.GetSearchableTexts(), out var score)
                ? new { Row = row, Score = score }
                : null)
            .Where(match => match is not null)
            .Select(match => match!)
            .OrderByDescending(match => match.Score)
            .ThenByDescending(match => match.Row.PlayCount)
            .ThenBy(match => match.Row.Title)
            .Select(match => match.Row)
            .ToList();

        var combinedItems = exactRows
            .Select(row => row.ToDto())
            .Concat(rankedFuzzyRows.Select(row => row.ToDto()))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<SearchTrackDto>(
            combinedItems,
            exactCount + rankedFuzzyRows.Count,
            page,
            pageSize);
    }
}
