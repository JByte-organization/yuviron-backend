using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services.Jamendo;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Jamendo.Services;

public class JamendoEntityResolver : IJamendoEntityResolver
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<string, Artist> _artistCache = new();
    private readonly ConcurrentDictionary<string, Album> _albumCache = new();

    public JamendoEntityResolver(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Artist> ResolveArtistAsync(string name, string jamendoId, CancellationToken cancellationToken)
    {
        if (_artistCache.TryGetValue(jamendoId, out var cachedArtist)) return cachedArtist;

        var mapping = await _context.ExternalMappings
            .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && m.ExternalId == jamendoId && m.EntityType == nameof(Artist), cancellationToken);

        if (mapping != null)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == mapping.InternalId, cancellationToken);
            if (artist != null)
            {
                _artistCache[jamendoId] = artist;
                return artist;
            }
            
            // If mapping exists but artist is missing (deleted or failed cleanup), 
            // remove the dead mapping and proceed to create a new one.
            _context.ExternalMappings.Remove(mapping);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var newArtist = Artist.Create(null, name, "Imported from Jamendo", null, null, default, _timeProvider.GetUtcNow().UtcDateTime);
        _context.Artists.Add(newArtist);
        _context.ExternalMappings.Add(ExternalMapping.Create(newArtist.Id, nameof(Artist), ExternalProvider.Jamendo, jamendoId));

        await _context.SaveChangesAsync(cancellationToken);
        _artistCache[jamendoId] = newArtist;
        return newArtist;
    }

    public async Task<Album> ResolveAlbumAsync(string title, string jamendoId, Guid artistId, Guid adminId, CancellationToken cancellationToken)
    {
        if (_albumCache.TryGetValue(jamendoId, out var cachedAlbum)) return cachedAlbum;

        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Singles" : title;

        var mapping = await _context.ExternalMappings
            .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && m.ExternalId == jamendoId && m.EntityType == nameof(Album), cancellationToken);

        if (mapping != null)
        {
            var album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == mapping.InternalId, cancellationToken);
            if (album != null)
            {
                _albumCache[jamendoId] = album;
                return album;
            }

            _context.ExternalMappings.Remove(mapping);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var releaseType = string.Equals(safeTitle, "Singles", StringComparison.OrdinalIgnoreCase) ? ReleaseType.Single : ReleaseType.Album;

        var newAlbum = Album.Create(safeTitle, "Imported from Jamendo", null, utcNow, releaseType, VisibilityStatus.Draft, null, new[] { artistId }, utcNow);

        _context.Albums.Add(newAlbum);
        _context.ExternalMappings.Add(ExternalMapping.Create(newAlbum.Id, nameof(Album), ExternalProvider.Jamendo, jamendoId));
        
        await _context.SaveChangesAsync(cancellationToken);
        _albumCache[jamendoId] = newAlbum;
        return newAlbum;
    }

    public async Task<int> GetNextAlbumPositionAsync(Guid albumId, CancellationToken cancellationToken)
    {
        var maxPos = await _context.Tracks
            .IgnoreQueryFilters() // See even deleted tracks for correct position increment
            .Where(t => t.AlbumId == albumId)
            .MaxAsync(t => (int?)t.AlbumPosition, cancellationToken) ?? 0;
            
        return maxPos + 1;
    }
}
