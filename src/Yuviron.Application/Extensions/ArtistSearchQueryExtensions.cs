using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Extensions;

public static class ArtistSearchQueryExtensions
{
    public static IQueryable<Artist> BuildPublicArtistSearchQuery(
        this IApplicationDbContext context,
        string searchTerm)
    {
        return context.Artists
            .AsNoTracking()
            .Where(a => !a.IsDeleted &&
                        a.Name.ToLower().Contains(searchTerm));
    }

    public static IQueryable<Artist> BuildPublicArtistFuzzyCandidateQuery(
        this IApplicationDbContext context,
        IReadOnlyCollection<string> fragments)
    {
        var baseQuery = context.Artists
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (fragments.Count == 0)
        {
            return baseQuery.Where(_ => false);
        }

        var predicates = fragments.Select(fragment => (Expression<Func<Artist, bool>>)(a =>
            a.Name.ToLower().Contains(fragment)));

        return baseQuery.WhereAny(predicates);
    }

    public static IOrderedQueryable<Artist> ApplyArtistSearchOrdering(this IQueryable<Artist> query)
    {
        return query
            .OrderByDescending(a => a.TotalPlays)
            .ThenBy(a => a.Name);
    }

    public static IQueryable<SearchArtistDto> SelectSearchArtistDtos(this IQueryable<Artist> query)
    {
        return query.Select(a => new SearchArtistDto(
            a.Id,
            a.Name,
            a.AvatarUrl
        ));
    }

    internal static IQueryable<ArtistSearchRow> SelectArtistSearchRows(this IQueryable<Artist> query)
    {
        return query.Select(a => new ArtistSearchRow(
            a.Id,
            a.Name,
            a.AvatarUrl,
            a.TotalPlays
        ));
    }
}

internal sealed record ArtistSearchRow(
    Guid Id,
    string Name,
    string? AvatarUrl,
    long TotalPlays)
{
    public SearchArtistDto ToDto() => new(Id, Name, AvatarUrl);

    public IEnumerable<string> GetSearchableTexts()
    {
        yield return Name;
    }
}
