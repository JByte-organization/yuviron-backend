using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services.Jamendo;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Jamendo.Services;

public class JamendoEntityResolver : IJamendoEntityResolver
{
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<string, Artist> _artistCache = new();
    private readonly ConcurrentDictionary<string, Album> _albumCache = new();

    public JamendoEntityResolver(ICatalogContext catalogContext, ISystemContext systemContext, TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
    }

    public async Task<Artist> ResolveArtistAsync(string name, string jamendoId, CancellationToken cancellationToken)
    {
        if (_artistCache.TryGetValue(jamendoId, out var cachedArtist)) return cachedArtist;

        var mapping = await _systemContext.ExternalMappings
            .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && m.ExternalId == jamendoId && m.EntityType == nameof(Artist), cancellationToken);

        if (mapping != null)
        {
            var artist = await _catalogContext.Artists.FirstOrDefaultAsync(a => a.Id == mapping.InternalId, cancellationToken);
            if (artist != null)
            {
                _artistCache[jamendoId] = artist;
                return artist;
            }
            
            // If mapping exists but artist is missing (deleted or failed cleanup), 
            // remove the dead mapping and proceed to create a new one.
            _systemContext.Remove(mapping);
            await _catalogContext.SaveChangesAsync(cancellationToken);
        }

        var newArtist = Artist.Create(null, name, "Imported from Jamendo", null, null, default, _timeProvider.GetUtcNow().UtcDateTime);
        _catalogContext.Add(newArtist);
        _systemContext.Add(ExternalMapping.Create(newArtist.Id, nameof(Artist), ExternalProvider.Jamendo, jamendoId));

        await _catalogContext.SaveChangesAsync(cancellationToken);
        _artistCache[jamendoId] = newArtist;
        return newArtist;
    }

    public async Task<Album> ResolveAlbumAsync(string title, string jamendoId, Guid artistId, Guid adminId, CancellationToken cancellationToken)
    {
        if (_albumCache.TryGetValue(jamendoId, out var cachedAlbum)) return cachedAlbum;

        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Singles" : title;

        var mapping = await _systemContext.ExternalMappings
            .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && m.ExternalId == jamendoId && m.EntityType == nameof(Album), cancellationToken);

        if (mapping != null)
        {
            var album = await _catalogContext.Albums.FirstOrDefaultAsync(a => a.Id == mapping.InternalId, cancellationToken);
            if (album != null)
            {
                _albumCache[jamendoId] = album;
                return album;
            }

            _systemContext.Remove(mapping);
            await _catalogContext.SaveChangesAsync(cancellationToken);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var releaseType = string.Equals(safeTitle, "Singles", StringComparison.OrdinalIgnoreCase) ? ReleaseType.Single : ReleaseType.Album;

        var newAlbum = Album.Create(safeTitle, "Imported from Jamendo", null, utcNow, releaseType, VisibilityStatus.Draft, null, new[] { artistId }, utcNow);

        _catalogContext.Add(newAlbum);
        _systemContext.Add(ExternalMapping.Create(newAlbum.Id, nameof(Album), ExternalProvider.Jamendo, jamendoId));
        
        await _catalogContext.SaveChangesAsync(cancellationToken);
        _albumCache[jamendoId] = newAlbum;
        return newAlbum;
    }

    public async Task<int> GetNextAlbumPositionAsync(Guid albumId, CancellationToken cancellationToken)
    {
        var maxPos = await _catalogContext.Tracks
            .IgnoreQueryFilters() // See even deleted tracks for correct position increment
            .Where(t => t.AlbumId == albumId)
            .MaxAsync(t => (int?)t.AlbumPosition, cancellationToken) ?? 0;
            
        return maxPos + 1;
    }
}
