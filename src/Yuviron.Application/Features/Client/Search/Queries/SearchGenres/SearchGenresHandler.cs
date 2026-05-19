using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Utilities;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Client.Search.Queries.SearchGenres;

public sealed class SearchGenresHandler : IRequestHandler<SearchGenresQuery, PaginatedList<SearchGenreMoodDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public SearchGenresHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<SearchGenreMoodDto>> Handle(SearchGenresQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page;
        var pageSize = request.PageSize;
        var searchTerm = SearchQueryNormalizer.Normalize(request.SearchTerm);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var requiredExactResults = (long)page * pageSize;
        var take = requiredExactResults > int.MaxValue ? int.MaxValue : (int)requiredExactResults;

        var exactGenresQuery = _context.BuildPublicGenreSearchQuery(searchTerm, utcNow);
        var exactMoodsQuery = _context.BuildPublicMoodSearchQuery(searchTerm, utcNow);

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm))
        {
            var genresCount = await exactGenresQuery.CountAsync(cancellationToken);
            var moodsCount = await exactMoodsQuery.CountAsync(cancellationToken);

            var genreRows = await exactGenresQuery
                .ApplyGenreSearchOrdering()
                .Take(take)
                .SelectGenreSearchRows()
                .ToListAsync(cancellationToken);

            var moodRows = await exactMoodsQuery
                .ApplyMoodSearchOrdering()
                .Take(take)
                .SelectMoodSearchRows()
                .ToListAsync(cancellationToken);

            var pagedItems = genreRows
                .Select(row => new SearchGenreMoodItem(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Genre))
                .Concat(moodRows.Select(row => new SearchGenreMoodItem(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Mood)))
                .OrderBy(item => item.Name)
                .ThenBy(item => item.Type)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(item => item.ToDto())
                .ToList();

            return new PaginatedList<SearchGenreMoodDto>(pagedItems, genresCount + moodsCount, page, pageSize);
        }

        var exactGenresCount = await exactGenresQuery.CountAsync(cancellationToken);
        var exactMoodsCount = await exactMoodsQuery.CountAsync(cancellationToken);
        var totalExactCount = exactGenresCount + exactMoodsCount;

        var exactGenreRows = await exactGenresQuery
            .ApplyGenreSearchOrdering()
            .Take(take)
            .SelectGenreSearchRows()
            .ToListAsync(cancellationToken);

        var exactMoodRows = await exactMoodsQuery
            .ApplyMoodSearchOrdering()
            .Take(take)
            .SelectMoodSearchRows()
            .ToListAsync(cancellationToken);

        var exactItems = exactGenreRows
            .Select(row => new SearchGenreMoodItem(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Genre))
            .Concat(exactMoodRows.Select(row => new SearchGenreMoodItem(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Mood)))
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Type)
            .ToList();

        if (totalExactCount >= requiredExactResults)
        {
            var pagedExactItems = exactItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(item => item.ToDto())
                .ToList();

            return new PaginatedList<SearchGenreMoodDto>(pagedExactItems, totalExactCount, page, pageSize);
        }

        var exactKeys = exactItems
            .Select(item => item.Key)
            .ToHashSet(StringComparer.Ordinal);

        var fragments = SearchFuzzyMatcher.BuildCandidateFragments(searchTerm);

        var fuzzyGenreRows = await _context
            .BuildPublicGenreFuzzyCandidateQuery(fragments, utcNow)
            .SelectGenreSearchRows()
            .ToListAsync(cancellationToken);

        var fuzzyMoodRows = await _context
            .BuildPublicMoodFuzzyCandidateQuery(fragments, utcNow)
            .SelectMoodSearchRows()
            .ToListAsync(cancellationToken);

        var rankedFuzzyItems = fuzzyGenreRows
            .Select(row => new SearchGenreMoodItem(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Genre))
            .Concat(fuzzyMoodRows.Select(row => new SearchGenreMoodItem(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Mood)))
            .Where(item => !exactKeys.Contains(item.Key))
            .Select(item => SearchFuzzyMatcher.TryGetScore(searchTerm, item.GetSearchableTexts(), out var score)
                ? new { Item = item, Score = score }
                : null)
            .Where(match => match is not null)
            .Select(match => match!)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Item.Name)
            .ThenBy(match => match.Item.Type)
            .Select(match => match.Item)
            .ToList();

        var combinedItems = exactItems
            .Concat(rankedFuzzyItems)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => item.ToDto())
            .ToList();

        return new PaginatedList<SearchGenreMoodDto>(
            combinedItems,
            totalExactCount + rankedFuzzyItems.Count,
            page,
            pageSize);
    }

    private sealed record SearchGenreMoodItem(
        Guid Id,
        string Name,
        string? CoverUrl,
        string Type)
    {
        public string Key => $"{Type}:{Id}";

        public SearchGenreMoodDto ToDto() => new(Id, Name, CoverUrl, Type);

        public IEnumerable<string> GetSearchableTexts()
        {
            yield return Name;
        }
    }
}
