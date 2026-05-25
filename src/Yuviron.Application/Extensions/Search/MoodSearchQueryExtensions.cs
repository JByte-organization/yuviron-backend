using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Extensions;

public static class MoodSearchQueryExtensions
{
    public static IQueryable<Mood> BuildPublicMoodSearchQuery(
        this IApplicationDbContext context,
        string searchTerm,
        DateTime utcNow)
    {
        var publicTracks = context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        return context.Moods
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .Where(m => publicTracks.Any(t => t.TrackMoods.Any(tm => tm.MoodId == m.Id)))
            .Where(m => m.Name.ToLower().Contains(searchTerm));
    }

    public static IQueryable<Mood> BuildPublicMoodFuzzyCandidateQuery(
        this IApplicationDbContext context,
        IReadOnlyCollection<string> fragments,
        DateTime utcNow)
    {
        var publicTracks = context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        var baseQuery = context.Moods
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .Where(m => publicTracks.Any(t => t.TrackMoods.Any(tm => tm.MoodId == m.Id)));

        if (fragments.Count == 0)
        {
            return baseQuery.Where(_ => false);
        }

        var predicates = fragments.Select(fragment => (Expression<Func<Mood, bool>>)(m =>
            m.Name.ToLower().Contains(fragment)));

        return baseQuery.WhereAny(predicates);
    }

    public static IOrderedQueryable<Mood> ApplyMoodSearchOrdering(this IQueryable<Mood> query)
    {
        return query.OrderBy(m => m.Name);
    }

    internal static IQueryable<MoodSearchRow> SelectMoodSearchRows(this IQueryable<Mood> query)
    {
        return query.Select(m => new MoodSearchRow(
            m.Id,
            m.Name,
            m.CoverUrl
        ));
    }
}

internal sealed record MoodSearchRow(
    Guid Id,
    string Name,
    string? CoverUrl)
{
    public IEnumerable<string> GetSearchableTexts()
    {
        yield return Name;
    }
}
