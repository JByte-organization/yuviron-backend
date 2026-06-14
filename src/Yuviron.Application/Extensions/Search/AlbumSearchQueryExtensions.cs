using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Extensions;

public static class AlbumSearchQueryExtensions
{
    public static IQueryable<Album> BuildPublicAlbumSearchQuery(
        this ICatalogContext catalogContext,
        string searchTerm,
        DateTime utcNow)
    {
        var publicTracks = catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        return catalogContext.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(a => publicTracks.Any(t => t.AlbumId == a.Id))
            .Where(a => a.Title.ToLower().Contains(searchTerm) ||
                        a.AlbumArtists.Any(aa => !aa.Artist.IsDeleted && aa.Artist.Name.ToLower().Contains(searchTerm)));
    }

    public static IQueryable<Album> BuildPublicAlbumFuzzyCandidateQuery(
        this ICatalogContext catalogContext,
        IReadOnlyCollection<string> fragments,
        DateTime utcNow)
    {
        var publicTracks = catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        var baseQuery = catalogContext.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(a => publicTracks.Any(t => t.AlbumId == a.Id));

        if (fragments.Count == 0)
        {
            return baseQuery.Where(_ => false);
        }

        var predicates = fragments.Select(fragment => (Expression<Func<Album, bool>>)(a =>
            a.Title.ToLower().Contains(fragment) ||
            a.AlbumArtists.Any(aa => !aa.Artist.IsDeleted && aa.Artist.Name.ToLower().Contains(fragment))));

        return baseQuery.WhereAny(predicates);
    }

    public static IOrderedQueryable<Album> ApplyAlbumSearchOrdering(this IQueryable<Album> query)
    {
        return query
            .OrderByDescending(a => a.ReleaseDate)
            .ThenByDescending(a => a.CreatedAt)
            .ThenBy(a => a.Title);
    }

    public static IQueryable<SearchAlbumDto> SelectSearchAlbumDtos(this IQueryable<Album> query)
    {
        return query.Select(a => new SearchAlbumDto(
            a.Id,
            a.Title,
            a.AlbumArtists.Where(aa => !aa.Artist.IsDeleted).Select(aa => new TrackArtistDto(aa.Artist.Id, aa.Artist.Name, aa.Role)),
            a.CoverUrl,
            a.ReleaseDate.Year,
            false
        ));
    }

    internal static IQueryable<AlbumSearchRow> SelectAlbumSearchRows(this IQueryable<Album> query)
    {
        return query.Select(a => new AlbumSearchRow(
            a.Id,
            a.Title,
            a.AlbumArtists.Where(aa => !aa.Artist.IsDeleted).Select(aa => new TrackArtistDto(aa.Artist.Id, aa.Artist.Name, aa.Role)),
            a.CoverUrl,
            a.ReleaseDate.Year,
            a.ReleaseDate,
            a.CreatedAt
        ));
    }
}

internal sealed record AlbumSearchRow(
    Guid Id,
    string Title,
    IEnumerable<TrackArtistDto> Artists,
    string? CoverUrl,
    int ReleaseYear,
    DateTime ReleaseDate,
    DateTime CreatedAt)
{
    public SearchAlbumDto ToDto() => new(Id, Title, Artists, CoverUrl, ReleaseYear);

    public IEnumerable<string> GetSearchableTexts()
    {
        yield return Title;

        foreach (var artist in Artists)
        {
            yield return artist.Name;
        }
    }
}
