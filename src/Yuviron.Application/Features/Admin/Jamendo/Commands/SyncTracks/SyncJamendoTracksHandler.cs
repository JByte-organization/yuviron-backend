using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

public sealed class SyncJamendoTracksHandler : IRequestHandler<SyncJamendoTracksCommand, int>
{
    private readonly IJamendoApiService _jamendoApi;
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly ILogger<SyncJamendoTracksHandler> _logger;

    public SyncJamendoTracksHandler(
        IJamendoApiService jamendoApi,
        IApplicationDbContext context,
        ISender sender,
        ILogger<SyncJamendoTracksHandler> logger)
    {
        _jamendoApi = jamendoApi;
        _context = context;
        _sender = sender;
        _logger = logger;
    }

    public async Task<int> Handle(SyncJamendoTracksCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начинаем синхронизацию {Limit} треков (Offset: {Offset})...", request.Limit, request.Offset);

        var jamendoTracks = await _jamendoApi.GetTracksAsync(request.Limit, request.Offset, cancellationToken);
        if (!jamendoTracks.Any()) return 0;

        var defaultGenre = await GetOrCreateGenreAsync("Jamendo Hits", cancellationToken);
        var defaultMood = await GetOrCreateMoodAsync("Various", cancellationToken);
        await _context.SaveChangesAsync(cancellationToken); 

        var localArtistCache = new Dictionary<string, Artist>();
        var localAlbumCache = new Dictionary<string, Album>();

        int syncedCount = 0;

        foreach (var jt in jamendoTracks)
        {
            try
            {
                bool hasNewPrerequisites = false;

                if (!localArtistCache.TryGetValue(jt.ArtistId, out var artist))
                {
                    artist = await GetOrCreateArtistAsync(jt.ArtistName, jt.ArtistId, cancellationToken);
                    localArtistCache[jt.ArtistId] = artist;
                    hasNewPrerequisites = true;
                }

                var safeAlbumTitle = string.IsNullOrWhiteSpace(jt.AlbumName) ? "Singles" : jt.AlbumName;
                if (!localAlbumCache.TryGetValue(jt.AlbumId, out var album))
                {
                    album = await GetOrCreateAlbumAsync(safeAlbumTitle, jt.AlbumId, artist.Id, cancellationToken);
                    localAlbumCache[jt.AlbumId] = album;
                    hasNewPrerequisites = true;
                }

                if (hasNewPrerequisites)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var hasMapping = await _context.ExternalMappings.AnyAsync(m => 
                    m.Provider == ExternalProvider.Jamendo && 
                    m.ExternalId == jt.Id && 
                    m.EntityType == nameof(Track), cancellationToken);
                    
                if (hasMapping)
                {
                    _logger.LogInformation("Jamendo track:{Id} is already downloaded. Skip.", jt.Id);
                    continue;
                }

                Guid? existingTrackId = null;

                if (!string.IsNullOrWhiteSpace(jt.Isrc))
                {
                    var trackByIsrc = await _context.Tracks.FirstOrDefaultAsync(t => t.Isrc == jt.Isrc, cancellationToken);
                    if (trackByIsrc != null)
                    {
                        existingTrackId = trackByIsrc.Id;
                        _logger.LogInformation("Track {Title} found via ISRC. Let's connect.", jt.Name);
                    }
                }

                if (existingTrackId.HasValue)
                {
                    var newMapping = ExternalMapping.Create(existingTrackId.Value, nameof(Track), ExternalProvider.Jamendo, jt.Id);
                    _context.ExternalMappings.Add(newMapping);
                    
                    syncedCount++;
                    continue;
                }

                int position = jt.Position > 0 ? jt.Position : 1;

                var audioKey = await _jamendoApi.DownloadFileToTempAsync(jt.AudioDownloadUrl, ".mp3", cancellationToken);
                var coverKey = await _jamendoApi.DownloadFileToTempAsync(jt.CoverUrl, ".jpg", cancellationToken);

                if (string.IsNullOrWhiteSpace(audioKey)) continue;

                var createTrackCmd = new CreateTrackCommand(
                    album.Id, position, jt.Name, false, audioKey, coverKey, VisibilityStatus.Published,
                    new List<Guid> { artist.Id }, new List<Guid> { defaultGenre.Id }, new List<Guid> { defaultMood.Id }, jt.Isrc
                );

                var trackId = await _sender.Send(createTrackCmd, cancellationToken);
                
                var trackMapping = ExternalMapping.Create(trackId, nameof(Track), ExternalProvider.Jamendo, jt.Id);
                _context.ExternalMappings.Add(trackMapping);
                

                syncedCount++;
                _logger.LogInformation("Track {TrackName} has been successfully sent for processing!", jt.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing Jamendo track:{Id}", jt.Id);
            }
        }

        if (syncedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return syncedCount;
    }


    private async Task<Genre> GetOrCreateGenreAsync(string name, CancellationToken cancellationToken)
    {
        var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
        if (genre == null)
        {
            genre = Genre.Create(name, null, DateTime.UtcNow);
            _context.Genres.Add(genre);
        }
        return genre;
    }

    private async Task<Mood> GetOrCreateMoodAsync(string name, CancellationToken cancellationToken)
    {
        var mood = await _context.Moods.FirstOrDefaultAsync(m => m.Name == name, cancellationToken);
        if (mood == null)
        {
            mood = Mood.Create(name, null, DateTime.UtcNow);
            _context.Moods.Add(mood);
        }
        return mood;
    }

    private async Task<Artist> GetOrCreateArtistAsync(string name, string jamendoId, CancellationToken ct)
    {
        var mapping = await _context.ExternalMappings
            .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && m.ExternalId == jamendoId && m.EntityType == nameof(Artist), ct);

        if (mapping != null)
            return await _context.Artists.FirstAsync(a => a.Id == mapping.InternalId, ct);

        var artist = Artist.Create(null, name, "Imported from Jamendo", null, null, default, DateTime.UtcNow);
        _context.Artists.Add(artist);
        
        var newMapping = ExternalMapping.Create(artist.Id, nameof(Artist), ExternalProvider.Jamendo, jamendoId);
        _context.ExternalMappings.Add(newMapping);

        return artist;
    }

    private async Task<Album> GetOrCreateAlbumAsync(string title, string jamendoId, Guid artistId, CancellationToken ct)
    {
        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Singles" : title; 

        var mapping = await _context.ExternalMappings
            .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && m.ExternalId == jamendoId && m.EntityType == nameof(Album), ct);

        if (mapping != null)
            return await _context.Albums.FirstAsync(a => a.Id == mapping.InternalId, ct);

        var album = Album.Create(safeTitle, "Imported from Jamendo", null, DateTime.UtcNow, VisibilityStatus.Published, null, new List<Guid> { artistId }, DateTime.UtcNow);
        _context.Albums.Add(album);
        
        var newMapping = ExternalMapping.Create(album.Id, nameof(Album), ExternalProvider.Jamendo, jamendoId);
        _context.ExternalMappings.Add(newMapping);

        return album;
    }
}