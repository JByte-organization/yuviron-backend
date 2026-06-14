using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Extensions;

public static class ArtistFilteringExtensions
{
    /// <summary>
    /// Returns tracks where the artist is a Primary/Main artist.
    /// </summary>
    public static IQueryable<Track> ForArtistMain(this IQueryable<Track> query, Guid artistId)
    {
        return query.Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId && ta.Role == ArtistRole.Main));
    }

    /// <summary>
    /// Returns tracks where the artist is a Guest (Featured) artist.
    /// </summary>
    public static IQueryable<Track> ForArtistAppearsOn(this IQueryable<Track> query, Guid artistId)
    {
        return query.Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId && (ta.Role == ArtistRole.Feat || ta.Role == ArtistRole.Producer)));
    }

    /// <summary>
    /// Returns any track where the artist is involved (old behavior).
    /// </summary>
    public static IQueryable<Track> ForArtist(this IQueryable<Track> query, Guid artistId)
    {
        return query.Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId));
    }

    /// <summary>
    /// Returns albums where the artist is a Primary/Main artist.
    /// </summary>
    public static IQueryable<Album> ForArtistMain(this IQueryable<Album> query, Guid artistId)
    {
        return query.Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == artistId && aa.Role == ArtistRole.Main));
    }

    /// <summary>
    /// Returns albums where the artist is a Guest (Featured) or Producer.
    /// </summary>
    public static IQueryable<Album> ForArtistAppearsOn(this IQueryable<Album> query, Guid artistId)
    {
        return query.Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == artistId && (aa.Role == ArtistRole.Feat || aa.Role == ArtistRole.Producer)));
    }

    public static IQueryable<Album> ForArtist(this IQueryable<Album> query, Guid artistId)
    {
        return query.Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == artistId));
    }
}
