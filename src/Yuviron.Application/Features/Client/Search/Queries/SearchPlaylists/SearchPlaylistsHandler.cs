using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Utilities;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

namespace Yuviron.Application.Features.Client.Search.Queries.SearchPlaylists;

public sealed class SearchPlaylistsHandler : IRequestHandler<SearchPlaylistsQuery, PaginatedList<SearchPlaylistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public SearchPlaylistsHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<SearchPlaylistDto>> Handle(SearchPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page;
        var pageSize = request.PageSize;
        var searchTerm = SearchQueryNormalizer.Normalize(request.SearchTerm);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var exactQuery = _context.BuildPublicPlaylistSearchQuery(searchTerm, utcNow);

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm))
        {
            return await exactQuery
                .ApplyPlaylistSearchOrdering()
                .SelectSearchPlaylistDtos()
                .ToPaginatedListAsync(page, pageSize, cancellationToken);
        }

        var exactCount = await exactQuery.CountAsync(cancellationToken);
        var requiredExactResults = (long)page * pageSize;

        if (exactCount >= requiredExactResults)
        {
            var items = await exactQuery
                .ApplyPlaylistSearchOrdering()
                .SelectSearchPlaylistDtos()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<SearchPlaylistDto>(items, exactCount, page, pageSize);
        }

        var exactRows = await exactQuery
            .ApplyPlaylistSearchOrdering()
            .Take(requiredExactResults > int.MaxValue ? int.MaxValue : (int)requiredExactResults)
            .SelectPlaylistSearchRows()
            .ToListAsync(cancellationToken);

        var exactIds = exactRows
            .Select(row => row.Id)
            .ToHashSet();

        var fragments = SearchFuzzyMatcher.BuildCandidateFragments(searchTerm);
        var fuzzyRows = await _context
            .BuildPublicPlaylistFuzzyCandidateQuery(fragments, utcNow)
            .SelectPlaylistSearchRows()
            .ToListAsync(cancellationToken);

        var rankedFuzzyRows = fuzzyRows
            .Where(row => !exactIds.Contains(row.Id))
            .Select(row => SearchFuzzyMatcher.TryGetScore(searchTerm, row.GetSearchableTexts(), out var score)
                ? new { Row = row, Score = score }
                : null)
            .Where(match => match is not null)
            .Select(match => match!)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Row.Title)
            .Select(match => match.Row)
            .ToList();

        var combinedItems = exactRows
            .Select(row => row.ToDto())
            .Concat(rankedFuzzyRows.Select(row => row.ToDto()))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<SearchPlaylistDto>(
            combinedItems,
            exactCount + rankedFuzzyRows.Count,
            page,
            pageSize);
    }
}
