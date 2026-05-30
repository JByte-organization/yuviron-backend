using Yuviron.Domain.Entities;

namespace Yuviron.Application.Extensions;

public static class ArtistFilteringExtensions
{
    public static IQueryable<Track> ForArtist(this IQueryable<Track> query, Guid artistId)
    {
        return query.Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId));
    }

    public static IQueryable<Album> ForArtist(this IQueryable<Album> query, Guid artistId)
    {
        return query.Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == artistId));
    }
}
