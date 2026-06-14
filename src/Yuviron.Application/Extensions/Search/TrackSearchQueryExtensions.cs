using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Extensions;

public static class TrackSearchQueryExtensions
{
    public static IQueryable<Track> BuildPublicTrackSearchQuery(
        this ICatalogContext catalogContext,
        string searchTerm,
        DateTime utcNow)
    {
        return catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => t.Title.ToLower().Contains(searchTerm) ||
                        t.TrackArtists.Any(ta => !ta.Artist.IsDeleted && ta.Artist.Name.ToLower().Contains(searchTerm)));
    }

    public static IQueryable<Track> BuildPublicTrackFuzzyCandidateQuery(
        this ICatalogContext catalogContext,
        IReadOnlyCollection<string> fragments,
        DateTime utcNow)
    {
        var baseQuery = catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        if (fragments.Count == 0)
        {
            return baseQuery.Where(_ => false);
        }

        var predicates = fragments.Select(fragment => (Expression<Func<Track, bool>>)(t =>
            t.Title.ToLower().Contains(fragment) ||
            t.TrackArtists.Any(ta => !ta.Artist.IsDeleted && ta.Artist.Name.ToLower().Contains(fragment))));

        return baseQuery.WhereAny(predicates);
    }

    public static IOrderedQueryable<Track> ApplyTrackSearchOrdering(this IQueryable<Track> query)
    {
        return query
            .OrderByDescending(t => t.PlayCount)
            .ThenBy(t => t.Title);
    }

    public static IQueryable<SearchTrackDto> SelectSearchTrackDtos(this IQueryable<Track> query)
    {
        return query.Select(t => new SearchTrackDto(
            t.Id,
            t.Title,
            t.TrackArtists
                .Where(ta => !ta.Artist.IsDeleted)
                .Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
            t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
            false
        ));
    }

    internal static IQueryable<TrackSearchRow> SelectTrackSearchRows(this IQueryable<Track> query)
    {
        return query.Select(t => new TrackSearchRow(
            t.Id,
            t.Title,
            t.TrackArtists
                .Where(ta => !ta.Artist.IsDeleted)
                .Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
            t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
            t.PlayCount
        ));
    }
}

internal sealed record TrackSearchRow(
    Guid Id,
    string Title,
    IEnumerable<TrackArtistDto> Artists,
    string? CoverUrl,
    long PlayCount)
{
    public SearchTrackDto ToDto() => new(Id, Title, Artists, CoverUrl);

    public IEnumerable<string> GetSearchableTexts()
    {
        yield return Title;

        foreach (var artist in Artists)
        {
            yield return artist.Name;
        }
    }
}
