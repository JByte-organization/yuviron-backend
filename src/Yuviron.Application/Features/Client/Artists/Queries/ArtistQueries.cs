using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries;

internal static class ArtistQueries
{
    public static async Task EnsureArtistExistsAsync(
        IApplicationDbContext context,
        Guid artistId,
        CancellationToken cancellationToken)
    {
        var exists = await context.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == artistId && !a.IsDeleted, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(nameof(Artist), artistId);
        }
    }

    public static IQueryable<Track> BuildPublicArtistTracksQuery(
        IApplicationDbContext context,
        Guid artistId,
        DateTime utcNow)
    {
        return context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId));
    }

    public static IQueryable<Album> BuildPublicArtistReleasesQuery(
        IApplicationDbContext context,
        Guid artistId,
        DateTime utcNow)
    {
        return context.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == artistId));
    }
}
