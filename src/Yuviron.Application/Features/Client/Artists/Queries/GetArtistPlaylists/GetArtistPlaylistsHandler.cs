using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistPlaylists;

public sealed class GetArtistPlaylistsHandler : IRequestHandler<GetArtistPlaylistsQuery, PaginatedList<ArtistPlaylistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetArtistPlaylistsHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<ArtistPlaylistDto>> Handle(GetArtistPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var artistExists = await _context.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId && !a.IsDeleted, cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var publicTracks = _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow);
        var publicArtistTrackIds = _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .ForArtist(request.ArtistId)
            .Select(t => t.Id);

        var projectedQuery = _context.Playlists
            .AsNoTracking()
            .Where(p => p.Visibility == PlaylistVisibility.Public)
            .Where(p => p.PlaylistTracks.Any(pt => publicArtistTrackIds.Contains(pt.TrackId)))
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.CoverUrl,
                p.IsEditorial,
                p.UpdatedAt,
                CreatorName = p.IsEditorial
                    ? "Yuviron"
                    : (p.User != null && p.User.Profile != null ? p.User.Profile.FirstName : "User"),
                TracksCount = p.PlaylistTracks.Count(pt => publicTracks.Any(t => t.Id == pt.TrackId)),
                ArtistTracksCount = p.PlaylistTracks.Count(pt => publicArtistTrackIds.Contains(pt.TrackId))
            })
            .OrderByDescending(p => p.ArtistTracksCount)
            .ThenByDescending(p => p.UpdatedAt)
            .ThenBy(p => p.Title)
            .Select(p => new ArtistPlaylistDto(
                p.Id,
                p.Title,
                p.CreatorName,
                p.CoverUrl,
                p.TracksCount,
                p.IsEditorial
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
