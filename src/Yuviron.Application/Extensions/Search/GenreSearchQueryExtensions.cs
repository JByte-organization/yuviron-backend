using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Extensions;

public static class GenreSearchQueryExtensions
{
    public static IQueryable<Genre> BuildPublicGenreSearchQuery(
        this ICatalogContext catalogContext,
        string searchTerm,
        DateTime utcNow)
    {
        var publicTracks = catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        return catalogContext.Genres
            .AsNoTracking()
            .Where(g => !g.IsDeleted &&
                        g.Name.ToLower().Contains(searchTerm) &&
                        publicTracks.Any(t => t.TrackGenres.Any(tg => tg.GenreId == g.Id)));
    }

    public static IQueryable<Genre> BuildPublicGenreFuzzyCandidateQuery(
        this ICatalogContext catalogContext,
        IReadOnlyCollection<string> fragments,
        DateTime utcNow)
    {
        var publicTracks = catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        var baseQuery = catalogContext.Genres
            .AsNoTracking()
            .Where(g => !g.IsDeleted &&
                        publicTracks.Any(t => t.TrackGenres.Any(tg => tg.GenreId == g.Id)));

        if (fragments.Count == 0)
        {
            return baseQuery.Where(_ => false);
        }

        var predicates = fragments.Select(fragment => (Expression<Func<Genre, bool>>)(g =>
            g.Name.ToLower().Contains(fragment)));

        return baseQuery.WhereAny(predicates);
    }

    public static IOrderedQueryable<Genre> ApplyGenreSearchOrdering(this IQueryable<Genre> query)
    {
        return query.OrderBy(g => g.Name);
    }

    internal static IQueryable<GenreSearchRow> SelectGenreSearchRows(this IQueryable<Genre> query)
    {
        return query.Select(g => new GenreSearchRow(
            g.Id,
            g.Name,
            g.CoverUrl
        ));
    }
}

internal sealed record GenreSearchRow(
    Guid Id,
    string Name,
    string? CoverUrl)
{
    public IEnumerable<string> GetSearchableTexts()
    {
        yield return Name;
    }
}
