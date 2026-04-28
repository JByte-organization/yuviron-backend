using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public sealed class GlobalSearchHandler : IRequestHandler<GlobalSearchQuery, GlobalSearchResponse>
{
    private readonly IApplicationDbContext _context;

    public GlobalSearchHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GlobalSearchResponse> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var searchTerm = request.Query.Trim().ToLower();

        var tracks = await SearchTracksAsync(searchTerm, request.Limit, cancellationToken);
        var artists = await SearchArtistsAsync(searchTerm, request.Limit, cancellationToken);
        var playlists = await SearchPlaylistsAsync(searchTerm, request.Limit, cancellationToken);

        return new GlobalSearchResponse(
            Tracks: tracks,
            Artists: artists,
            Playlists: playlists
        );
    }

    private async Task<List<SearchResultItemDto>> SearchTracksAsync(string searchTerm, int limit, CancellationToken ct)
    {
        var dbTracks = await _context.Tracks
            .AsNoTracking()
            .Where(t => !t.IsDeleted && 
                       (t.Title.ToLower().Contains(searchTerm) || 
                        t.TrackArtists.Any(ta => ta.Artist.Name.ToLower().Contains(searchTerm))))
            .OrderByDescending(t => t.PlayCount)
            .ThenBy(t => t.Title)
            .Take(limit)
            .Select(t => new
            {
                t.Id,
                t.Title,
                ArtistNames = t.TrackArtists.Select(ta => ta.Artist.Name).ToList(), 
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null)
            })
            .ToListAsync(ct);

        return dbTracks
            .Select(t => new SearchResultItemDto(
                t.Id,
                t.Title,
                string.Join(", ", t.ArtistNames), 
                t.CoverUrl,
                "track"
            ))
            .ToList();
    }

    private async Task<List<SearchResultItemDto>> SearchArtistsAsync(string searchTerm, int limit, CancellationToken ct)
    {
        var dbArtists = await _context.Artists
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.Name.ToLower().Contains(searchTerm))
            .OrderByDescending(a => a.TotalPlays)
            .ThenBy(a => a.Name)
            .Take(limit)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.AvatarUrl
            })
            .ToListAsync(ct);

        return dbArtists
            .Select(a => new SearchResultItemDto(
                a.Id,
                a.Name,
                null,
                a.AvatarUrl,
                "artist"
            ))
            .ToList();
    }

    private async Task<List<SearchResultItemDto>> SearchPlaylistsAsync(string searchTerm, int limit, CancellationToken ct)
    {
        var dbPlaylists = await _context.Playlists
            .AsNoTracking()
            .Where(p => !p.IsDeleted && 
                        p.Visibility == PlaylistVisibility.Public && 
                        p.Title.ToLower().Contains(searchTerm))
            .OrderBy(p => p.Title)
            .Take(limit)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.IsEditorial,
                OwnerName = p.User != null && p.User.Profile != null ? p.User.Profile.FirstName : null,
                p.CoverUrl
            })
            .ToListAsync(ct);

        return dbPlaylists
            .Select(p => new SearchResultItemDto(
                p.Id,
                p.Title,
                p.IsEditorial ? "Yuviron" : (p.OwnerName ?? "User"),
                p.CoverUrl,
                "playlist"
            ))
            .ToList();
    }
}