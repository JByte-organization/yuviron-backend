using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Utilities;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

namespace Yuviron.Application.Features.Client.Search.Queries.SearchAlbums;

public sealed class SearchAlbumsHandler : IRequestHandler<SearchAlbumsQuery, PaginatedList<SearchAlbumDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public SearchAlbumsHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<SearchAlbumDto>> Handle(SearchAlbumsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page;
        var pageSize = request.PageSize;
        var searchTerm = SearchQueryNormalizer.Normalize(request.SearchTerm);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var exactQuery = _context.BuildPublicAlbumSearchQuery(searchTerm, utcNow);

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm))
        {
            return await exactQuery
                .ApplyAlbumSearchOrdering()
                .SelectSearchAlbumDtos()
                .ToPaginatedListAsync(page, pageSize, cancellationToken);
        }

        var exactCount = await exactQuery.CountAsync(cancellationToken);
        var requiredExactResults = (long)page * pageSize;

        if (exactCount >= requiredExactResults)
        {
            var items = await exactQuery
                .ApplyAlbumSearchOrdering()
                .SelectSearchAlbumDtos()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<SearchAlbumDto>(items, exactCount, page, pageSize);
        }

        var exactRows = await exactQuery
            .ApplyAlbumSearchOrdering()
            .Take(requiredExactResults > int.MaxValue ? int.MaxValue : (int)requiredExactResults)
            .SelectAlbumSearchRows()
            .ToListAsync(cancellationToken);

        var exactIds = exactRows
            .Select(row => row.Id)
            .ToHashSet();

        var fragments = SearchFuzzyMatcher.BuildCandidateFragments(searchTerm);
        var fuzzyRows = await _context
            .BuildPublicAlbumFuzzyCandidateQuery(fragments, utcNow)
            .SelectAlbumSearchRows()
            .ToListAsync(cancellationToken);

        var rankedFuzzyRows = fuzzyRows
            .Where(row => !exactIds.Contains(row.Id))
            .Select(row => SearchFuzzyMatcher.TryGetScore(searchTerm, row.GetSearchableTexts(), out var score)
                ? new { Row = row, Score = score }
                : null)
            .Where(match => match is not null)
            .Select(match => match!)
            .OrderByDescending(match => match.Score)
            .ThenByDescending(match => match.Row.ReleaseDate)
            .ThenByDescending(match => match.Row.CreatedAt)
            .ThenBy(match => match.Row.Title)
            .Select(match => match.Row)
            .ToList();

        var combinedItems = exactRows
            .Select(row => row.ToDto())
            .Concat(rankedFuzzyRows.Select(row => row.ToDto()))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<SearchAlbumDto>(
            combinedItems,
            exactCount + rankedFuzzyRows.Count,
            page,
            pageSize);
    }
}
