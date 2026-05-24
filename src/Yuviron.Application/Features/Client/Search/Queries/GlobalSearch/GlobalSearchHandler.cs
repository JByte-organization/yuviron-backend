using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Utilities;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Client.Search.Queries.SearchGenres;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public sealed class GlobalSearchHandler : IRequestHandler<GlobalSearchQuery, GlobalSearchResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GlobalSearchHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<GlobalSearchResponse> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var searchTerm = SearchQueryNormalizer.Normalize(request.Query);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var trackResult = await SearchTracksAsync(searchTerm, utcNow, request.Limit, cancellationToken);
        var artistResult = await SearchArtistsAsync(searchTerm, request.Limit, cancellationToken);
        var albumResult = await SearchAlbumsAsync(searchTerm, utcNow, request.Limit, cancellationToken);
        var playlistResult = await SearchPlaylistsAsync(searchTerm, utcNow, request.Limit, cancellationToken);
        var genreResult = await SearchGenresAsync(searchTerm, utcNow, request.Limit, cancellationToken);

        var total = trackResult.TotalCount +
                    artistResult.TotalCount +
                    albumResult.TotalCount +
                    playlistResult.TotalCount +
                    genreResult.TotalCount;

        return new GlobalSearchResponse(
            trackResult.Items,
            artistResult.Items,
            albumResult.Items,
            playlistResult.Items,
            genreResult.Items,
            total);
    }

    private async Task<SearchSectionResult<SearchTrackDto>> SearchTracksAsync(
        string searchTerm,
        DateTime utcNow,
        int limit,
        CancellationToken cancellationToken)
    {
        var exactQuery = _context.BuildPublicTrackSearchQuery(searchTerm, utcNow);
        var exactCount = await exactQuery.CountAsync(cancellationToken);
        var exactItems = await exactQuery
            .ApplyTrackSearchOrdering()
            .Take(limit)
            .SelectSearchTrackDtos()
            .ToListAsync(cancellationToken);

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm) || exactCount >= limit)
        {
            return new SearchSectionResult<SearchTrackDto>(exactItems, exactCount);
        }

        var exactIds = exactItems.Select(item => item.Id).ToHashSet();
        var fragments = SearchFuzzyMatcher.BuildCandidateFragments(searchTerm);
        var fuzzyRows = await _context
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

        var items = exactItems
            .Concat(rankedFuzzyRows
                .Take(limit - exactItems.Count)
                .Select(row => row.ToDto()))
            .ToList();

        return new SearchSectionResult<SearchTrackDto>(items, exactCount + rankedFuzzyRows.Count);
    }

    private async Task<SearchSectionResult<SearchArtistDto>> SearchArtistsAsync(
        string searchTerm,
        int limit,
        CancellationToken cancellationToken)
    {
        var exactQuery = _context.BuildPublicArtistSearchQuery(searchTerm);
        var exactCount = await exactQuery.CountAsync(cancellationToken);
        var exactItems = await exactQuery
            .ApplyArtistSearchOrdering()
            .Take(limit)
            .SelectSearchArtistDtos()
            .ToListAsync(cancellationToken);

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm) || exactCount >= limit)
        {
            return new SearchSectionResult<SearchArtistDto>(exactItems, exactCount);
        }

        var exactIds = exactItems.Select(item => item.Id).ToHashSet();
        var fragments = SearchFuzzyMatcher.BuildCandidateFragments(searchTerm);
        var fuzzyRows = await _context
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

        var items = exactItems
            .Concat(rankedFuzzyRows
                .Take(limit - exactItems.Count)
                .Select(row => row.ToDto()))
            .ToList();

        return new SearchSectionResult<SearchArtistDto>(items, exactCount + rankedFuzzyRows.Count);
    }

    private async Task<SearchSectionResult<SearchAlbumDto>> SearchAlbumsAsync(
        string searchTerm,
        DateTime utcNow,
        int limit,
        CancellationToken cancellationToken)
    {
        var exactQuery = _context.BuildPublicAlbumSearchQuery(searchTerm, utcNow);
        var exactCount = await exactQuery.CountAsync(cancellationToken);
        var exactItems = await exactQuery
            .ApplyAlbumSearchOrdering()
            .Take(limit)
            .SelectSearchAlbumDtos()
            .ToListAsync(cancellationToken);

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm) || exactCount >= limit)
        {
            return new SearchSectionResult<SearchAlbumDto>(exactItems, exactCount);
        }

        var exactIds = exactItems.Select(item => item.Id).ToHashSet();
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

        var items = exactItems
            .Concat(rankedFuzzyRows
                .Take(limit - exactItems.Count)
                .Select(row => row.ToDto()))
            .ToList();

        return new SearchSectionResult<SearchAlbumDto>(items, exactCount + rankedFuzzyRows.Count);
    }

    private async Task<SearchSectionResult<SearchPlaylistDto>> SearchPlaylistsAsync(
        string searchTerm,
        DateTime utcNow,
        int limit,
        CancellationToken cancellationToken)
    {
        var exactQuery = _context.BuildPublicPlaylistSearchQuery(searchTerm, utcNow);
        var exactCount = await exactQuery.CountAsync(cancellationToken);
        var exactItems = await exactQuery
            .ApplyPlaylistSearchOrdering()
            .Take(limit)
            .SelectSearchPlaylistDtos()
            .ToListAsync(cancellationToken);

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm) || exactCount >= limit)
        {
            return new SearchSectionResult<SearchPlaylistDto>(exactItems, exactCount);
        }

        var exactIds = exactItems.Select(item => item.Id).ToHashSet();
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

        var items = exactItems
            .Concat(rankedFuzzyRows
                .Take(limit - exactItems.Count)
                .Select(row => row.ToDto()))
            .ToList();

        return new SearchSectionResult<SearchPlaylistDto>(items, exactCount + rankedFuzzyRows.Count);
    }

    private async Task<SearchSectionResult<SearchGenreMoodDto>> SearchGenresAsync(
        string searchTerm,
        DateTime utcNow,
        int limit,
        CancellationToken cancellationToken)
    {
        var exactGenresQuery = _context.BuildPublicGenreSearchQuery(searchTerm, utcNow);
        var exactMoodsQuery = _context.BuildPublicMoodSearchQuery(searchTerm, utcNow);

        var exactGenresCount = await exactGenresQuery.CountAsync(cancellationToken);
        var exactMoodsCount = await exactMoodsQuery.CountAsync(cancellationToken);
        var totalExactCount = exactGenresCount + exactMoodsCount;

        var exactGenreRows = await exactGenresQuery
            .ApplyGenreSearchOrdering()
            .Take(limit)
            .SelectGenreSearchRows()
            .ToListAsync(cancellationToken);

        var exactMoodRows = await exactMoodsQuery
            .ApplyMoodSearchOrdering()
            .Take(limit)
            .SelectMoodSearchRows()
            .ToListAsync(cancellationToken);

        var exactItems = exactGenreRows
            .Select(row => new SearchGenreMoodDto(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Genre))
            .Concat(exactMoodRows.Select(row => new SearchGenreMoodDto(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Mood)))
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Type)
            .Take(limit)
            .ToList();

        if (!SearchFuzzyMatcher.ShouldUseFuzzy(searchTerm) || totalExactCount >= limit)
        {
            return new SearchSectionResult<SearchGenreMoodDto>(exactItems, totalExactCount);
        }

        var exactKeys = exactItems
            .Select(item => $"{item.Type}:{item.Id}")
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
            .Select(row => new SearchGenreMoodDto(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Genre))
            .Concat(fuzzyMoodRows.Select(row => new SearchGenreMoodDto(row.Id, row.Name, row.CoverUrl, SearchEntityTypes.Mood)))
            .Where(item => !exactKeys.Contains($"{item.Type}:{item.Id}"))
            .Select(item => SearchFuzzyMatcher.TryGetScore(searchTerm, new[] { item.Name }, out var score)
                ? new { Item = item, Score = score }
                : null)
            .Where(match => match is not null)
            .Select(match => match!)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Item.Name)
            .ThenBy(match => match.Item.Type)
            .Select(match => match.Item)
            .ToList();

        var items = exactItems
            .Concat(rankedFuzzyItems.Take(limit - exactItems.Count))
            .ToList();

        return new SearchSectionResult<SearchGenreMoodDto>(items, totalExactCount + rankedFuzzyItems.Count);
    }

    private sealed record SearchSectionResult<T>(List<T> Items, int TotalCount);
}
