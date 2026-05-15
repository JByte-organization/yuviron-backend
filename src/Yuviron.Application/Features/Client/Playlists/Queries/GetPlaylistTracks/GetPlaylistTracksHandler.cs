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
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public GetPlaylistTracksHandler(IApplicationDbContext context, ICacheService cache, ICurrentUserService currentUser)
    {
        _context = context;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<PlaylistTrackItemClientDto>> Handle(GetPlaylistTracksQuery request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Playlist", request.PlaylistId);

        if (playlist.Visibility == PlaylistVisibility.Private && playlist.UserId != _currentUser.UserId)
        {
            throw new ForbiddenException("This playlist is private.");
        }

        bool isCustomSort = !string.IsNullOrWhiteSpace(request.SortBy) && 
                            !request.SortBy.Equals("Position", StringComparison.OrdinalIgnoreCase);

        if (isCustomSort)
        {
            return await GetTracksFromDbAsync(request, cancellationToken);
        }

        var redisKey = $"playlist:{request.PlaylistId}:tracks";
        long startIndex = (request.Page - 1) * request.PageSize;
        long stopIndex = startIndex + request.PageSize - 1;

        var tracksWithScores = await _cache.SortedSetRangeByRankWithScoresAsync(redisKey, startIndex, stopIndex, cancellationToken);
        var totalTracksCount = await _cache.SortedSetLengthAsync(redisKey, cancellationToken);

        if (tracksWithScores.Count == 0 && totalTracksCount == 0)
        {
            await _cache.SetAddAsync("missing_cache:playlists", request.PlaylistId.ToString(), cancellationToken);
            return await GetTracksFromDbAsync(request, cancellationToken);
        }

        var trackIds = tracksWithScores.Keys.Select(id => Guid.Parse(id)).ToList();
        var tracks = await _context.Tracks
            .AsNoTracking()
            .Include(t => t.Album)
            .Include(t => t.TrackArtists).ThenInclude(ta => ta.Artist)
            .Where(t => trackIds.Contains(t.Id))
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

        return new PaginatedList<PlaylistTrackItemClientDto>(dtos, (int)totalTracksCount, request.Page, request.PageSize);
    }

    private async Task<PaginatedList<PlaylistTrackItemClientDto>> GetTracksFromDbAsync(GetPlaylistTracksQuery request, CancellationToken ct)
    {
        var query = _context.PlaylistTracks
            .AsNoTracking()
            .Where(pt => pt.PlaylistId == request.PlaylistId);

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(PlaylistTrack.Position),
            defaultDesc: false,
            mapping: new Dictionary<string, Expression<Func<PlaylistTrack, object>>>
            {
                ["Title"] = pt => pt.Track.Title,
                ["DurationMs"] = pt => pt.Track.DurationMs,
                ["AddedAt"] = pt => pt.AddedAt,
                ["Position"] = pt => pt.Position
            });

        var projectedQuery = sortedQuery.Select(pt => new PlaylistTrackItemClientDto(
            pt.TrackId, 
            pt.Track.Title,
            pt.Track.TrackArtists.Select(ta => new SimpleArtistDto(ta.ArtistId, ta.Artist.Name)).ToList(),
            pt.Track.AlbumId, 
            pt.Track.CoverUrl ?? (pt.Track.Album != null ? pt.Track.Album.CoverUrl : null),
            pt.Track.DurationMs, 
            pt.Position, 
            pt.AddedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, ct);
    }

    private static PlaylistTrackItemClientDto MapToDto(Track track, double position, DateTime addedAt)
    {
        return new PlaylistTrackItemClientDto(
            track.Id, 
            track.Title,
            track.TrackArtists.Select(ta => new SimpleArtistDto(ta.ArtistId, ta.Artist.Name)).ToList(),
            track.AlbumId, 
            track.CoverUrl ?? track.Album?.CoverUrl,
            track.DurationMs, 
            position, 
            addedAt
        );
    }
}