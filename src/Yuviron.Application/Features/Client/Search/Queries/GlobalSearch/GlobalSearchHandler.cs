using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public sealed class GlobalSearchHandler : IRequestHandler<GlobalSearchQuery, GlobalSearchResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GlobalSearchHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<GlobalSearchResponse> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var searchTerm = request.Query.Trim().ToLower();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var tracks = await SearchTracksAsync(searchTerm, request.Limit, utcNow, cancellationToken);
        var artists = await SearchArtistsAsync(searchTerm, request.Limit, cancellationToken);
        var playlists = await SearchPlaylistsAsync(searchTerm, request.Limit, cancellationToken);

        return new GlobalSearchResponse(tracks, artists, playlists);
    }

    private async Task<List<SearchTrackDto>> SearchTracksAsync(string searchTerm, int limit, DateTime utcNow, CancellationToken ct)
    {
        var dbTracks = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow) 
            .Where(t => t.Title.Contains(searchTerm) || 
                        t.TrackArtists.Any(ta => !ta.Artist.IsDeleted && ta.Artist.Name.Contains(searchTerm)))
            .OrderByDescending(t => t.PlayCount)
            .ThenBy(t => t.Title)
            .Take(limit)
            .Select(t => new SearchTrackDto(
                t.Id,
                t.Title,
                t.TrackArtists
                    
                    .Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)), 
                t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null)
            ))
            .ToListAsync(ct);

        return dbTracks;
    }

    private async Task<List<SearchArtistDto>> SearchArtistsAsync(string searchTerm, int limit, CancellationToken ct)
    {
        var dbArtists = await _context.Artists
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.Name.Contains(searchTerm))
            .OrderByDescending(a => a.TotalPlays)
            .ThenBy(a => a.Name)
            .Take(limit)
            .Select(a => new SearchArtistDto(
                a.Id,
                a.Name,
                a.AvatarUrl
            ))
            .ToListAsync(ct);

        return dbArtists;
    }

    private async Task<List<SearchPlaylistDto>> SearchPlaylistsAsync(string searchTerm, int limit, CancellationToken ct)
    {
        var dbPlaylists = await _context.Playlists
            .AsNoTracking()
            .Where(p => !p.IsDeleted && 
                        p.Visibility == PlaylistVisibility.Public && 
                        p.Title.Contains(searchTerm))
            .OrderBy(p => p.Title)
            .Take(limit)
            .Select(p => new SearchPlaylistDto(
                p.Id,
                p.Title,
                p.IsEditorial ? "Yuviron" : (p.User != null && p.User.Profile != null ? p.User.Profile.FirstName : "User"),
                p.CoverUrl
            ))
            .ToListAsync(ct);

        return dbPlaylists;
    }
}
