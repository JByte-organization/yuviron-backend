using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistTracks;

public sealed class GetPlaylistTracksHandler : IRequestHandler<GetPlaylistTracksQuery, PaginatedList<PlaylistTrackItemClientDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public GetPlaylistTracksHandler(ICatalogContext catalogContext, ILibraryContext libraryContext, ICacheService cache, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<PlaylistTrackItemClientDto>> Handle(GetPlaylistTracksQuery request, CancellationToken cancellationToken)
    {
        var playlist = await _libraryContext.Playlists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId , cancellationToken)
            ?? throw new NotFoundException("Playlist", request.PlaylistId);

        if (playlist.Visibility == PlaylistVisibility.Private && playlist.UserId != _currentUser.UserId)
            throw new ForbiddenException("This playlist is private.");

        if (!string.IsNullOrWhiteSpace(request.SortBy) && !request.SortBy.Equals("Position", StringComparison.OrdinalIgnoreCase))
        {
            var dbTracks = await GetTracksFromDbAsync(request, cancellationToken);
            return await dbTracks.EnrichWithCacheAsync(_cache, _currentUser.UserId, "saved_tracks", x => x.TrackId, (x, saved) => x with { IsSaved = saved }, cancellationToken);
        }

        var redisKey = $"playlist:{request.PlaylistId}:tracks";
        long startIndex = (request.Page - 1) * request.PageSize;
        long stopIndex = startIndex + request.PageSize - 1;

        var tracksWithScores = await _cache.SortedSetRangeByRankWithScoresAsync(redisKey, startIndex, stopIndex, cancellationToken);
        var totalTracksCount = await _cache.SortedSetLengthAsync(redisKey, cancellationToken);

        if (tracksWithScores.Count == 0 && totalTracksCount == 0)
        {
            await _cache.SetAddAsync("missing_cache:playlists", request.PlaylistId.ToString(), cancellationToken);
            var dbTracks = await GetTracksFromDbAsync(request, cancellationToken);
            return await dbTracks.EnrichWithCacheAsync(_cache, _currentUser.UserId, "saved_tracks", x => x.TrackId, (x, saved) => x with { IsSaved = saved }, cancellationToken);
        }

        var trackIds = tracksWithScores.Keys.Select(id => Guid.Parse(id)).ToList();
        var tracks = await _catalogContext.Tracks
            .AsNoTracking()
            .Include(t => t.Album)
            .Include(t => t.TrackArtists).ThenInclude(ta => ta.Artist)
            .Where(t => trackIds.Contains(t.Id) )
            .ToListAsync(cancellationToken);

        var dtos = tracksWithScores.Keys
            .Select(trackIdStr => {
                var trackId = Guid.Parse(trackIdStr);
                var track = tracks.FirstOrDefault(t => t.Id == trackId);
                return track != null ? MapToDto(track, (int)tracksWithScores[trackIdStr], track.CreatedAt) : null;
            })
            .Where(d => d != null)
            .Cast<PlaylistTrackItemClientDto>()
            .ToList();

        var pagedList = new PaginatedList<PlaylistTrackItemClientDto>(dtos, (int)totalTracksCount, request.Page, request.PageSize);
        return await pagedList.EnrichWithCacheAsync(_cache, _currentUser.UserId, "saved_tracks", x => x.TrackId, (x, saved) => x with { IsSaved = saved }, cancellationToken);
    }

    private async Task<PaginatedList<PlaylistTrackItemClientDto>> GetTracksFromDbAsync(GetPlaylistTracksQuery request, CancellationToken ct)
    {
        var query = _libraryContext.PlaylistTracks
            .AsNoTracking()
            .Where(pt => pt.PlaylistId == request.PlaylistId && !pt.Track.IsDeleted);

        var sortedQuery = query.ApplySorting(request.SortBy, request.SortOrder, defaultSortBy: nameof(PlaylistTrack.Position), defaultDesc: false,
            mapping: new Dictionary<string, Expression<Func<PlaylistTrack, object>>> { ["Title"] = pt => pt.Track.Title, ["DurationMs"] = pt => pt.Track.DurationMs, ["AddedAt"] = pt => pt.AddedAt, ["Position"] = pt => pt.Position });

        var projectedQuery = sortedQuery.Select(pt => new PlaylistTrackItemClientDto(
            pt.TrackId, pt.Track.Title, pt.Track.TrackArtists.Select(ta => new SimpleArtistDto(ta.ArtistId, ta.Artist.Name)).ToList(),
            pt.Track.AlbumId, pt.Track.CoverUrl ?? (pt.Track.Album != null ? pt.Track.Album.CoverUrl : null),
            pt.Track.DurationMs, pt.Position, pt.AddedAt,
            false 
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, ct);
    }

    private static PlaylistTrackItemClientDto MapToDto(Track track, double position, DateTime addedAt)
    {
        return new PlaylistTrackItemClientDto(
            track.Id, track.Title, track.TrackArtists.Select(ta => new SimpleArtistDto(ta.ArtistId, ta.Artist.Name)).ToList(),
            track.AlbumId, track.CoverUrl ?? track.Album?.CoverUrl, track.DurationMs, position, addedAt,
            false
        );
    }
}