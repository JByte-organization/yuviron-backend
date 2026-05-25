using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;
using Yuviron.Domain.Enums;
using DomainPlaylist = Yuviron.Domain.Entities.Playlist;

namespace Yuviron.Application.Extensions;

public static class PlaylistSearchQueryExtensions
{
    public static IQueryable<DomainPlaylist> BuildPublicPlaylistSearchQuery(
        this IApplicationDbContext context,
        string searchTerm,
        DateTime utcNow)
    {
        var publicTracks = context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        return context.Playlists
            .AsNoTracking()
            .Where(p => !p.IsDeleted &&
                        p.Visibility == PlaylistVisibility.Public &&
                        p.PlaylistTracks.Any(pt => publicTracks.Any(t => t.Id == pt.TrackId)) &&
                        p.Title.ToLower().Contains(searchTerm));
    }

    public static IQueryable<DomainPlaylist> BuildPublicPlaylistFuzzyCandidateQuery(
        this IApplicationDbContext context,
        IReadOnlyCollection<string> fragments,
        DateTime utcNow)
    {
        var publicTracks = context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);

        var baseQuery = context.Playlists
            .AsNoTracking()
            .Where(p => !p.IsDeleted &&
                        p.Visibility == PlaylistVisibility.Public &&
                        p.PlaylistTracks.Any(pt => publicTracks.Any(t => t.Id == pt.TrackId)));

        if (fragments.Count == 0)
        {
            return baseQuery.Where(_ => false);
        }

        var predicates = fragments.Select(fragment => (Expression<Func<DomainPlaylist, bool>>)(p =>
            p.Title.ToLower().Contains(fragment)));

        return baseQuery.WhereAny(predicates);
    }

    public static IOrderedQueryable<DomainPlaylist> ApplyPlaylistSearchOrdering(this IQueryable<DomainPlaylist> query)
    {
        return query.OrderBy(p => p.Title);
    }

    public static IQueryable<SearchPlaylistDto> SelectSearchPlaylistDtos(this IQueryable<DomainPlaylist> query)
    {
        return query.Select(p => new SearchPlaylistDto(
            p.Id,
            p.Title,
            p.IsEditorial ? "Yuviron" : (p.User != null && p.User.Profile != null ? p.User.Profile.FirstName : "User"),
            p.CoverUrl
        ));
    }

    internal static IQueryable<PlaylistSearchRow> SelectPlaylistSearchRows(this IQueryable<DomainPlaylist> query)
    {
        return query.Select(p => new PlaylistSearchRow(
            p.Id,
            p.Title,
            p.IsEditorial ? "Yuviron" : (p.User != null && p.User.Profile != null ? p.User.Profile.FirstName : "User"),
            p.CoverUrl
        ));
    }
}

internal sealed record PlaylistSearchRow(
    Guid Id,
    string Title,
    string CreatorName,
    string? CoverUrl)
{
    public SearchPlaylistDto ToDto() => new(Id, Title, CreatorName, CoverUrl);

    public IEnumerable<string> GetSearchableTexts()
    {
        yield return Title;
    }
}
