using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Models;
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
    private readonly TimeProvider _timeProvider;

    public SyncJamendoTracksHandler(
        IJamendoApiService jamendoApi,
        IApplicationDbContext context,
        ISender sender,
        ILogger<SyncJamendoTracksHandler> logger,
        TimeProvider timeProvider)
    {
        _jamendoApi = jamendoApi;
        _context = context;
        _sender = sender;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<int> Handle(SyncJamendoTracksCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начинаем синхронизацию {Limit} треков (Offset: {Offset})...", request.Limit, request.Offset);

        var jamendoTracks = await _jamendoApi.GetTracksAsync(request.Limit, request.Offset, cancellationToken);
        if (!jamendoTracks.Any()) return 0;

        var localArtistCache = new Dictionary<string, Artist>();
        var localAlbumCache = new Dictionary<string, Album>();
        var downloadedCovers = new Dictionary<string, string>();
        var localGenreCache = new Dictionary<string, Guid>();
        var localMoodCache = new Dictionary<string, Guid>();

        var currentAlbumPositions = new Dictionary<Guid, int>();
        int syncedCount = 0;

        foreach (var jt in jamendoTracks)
        {
            try
            {
                var existingMapping = await _context.ExternalMappings
                    .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && 
                                              m.ExternalId == jt.Id && 
                                              m.EntityType == nameof(Track), cancellationToken);

                if (existingMapping != null)
                {
                    var mappedTrack = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == existingMapping.InternalId, cancellationToken);
                    
                    if (mappedTrack == null || mappedTrack.IsDeleted)
                    {
                        _context.ExternalMappings.Remove(existingMapping);
                        await _context.SaveChangesAsync(cancellationToken);
                        _logger.LogWarning("Мертвый маппинг найден и удален. Скачиваем трек {TrackName} заново.", jt.Name);
                    }
                    else
                    {
                        _logger.LogInformation("Jamendo track:{Id} is already downloaded and healthy. Skip.", jt.Id);
                        continue;
                    }
                }

                bool hasNewPrerequisites = false;
                string? coverKey = null;
                
                if (!string.IsNullOrWhiteSpace(jt.CoverUrl))
                {
                    if (!downloadedCovers.TryGetValue(jt.CoverUrl, out coverKey))
                    {
                        coverKey = await _jamendoApi.DownloadFileToTempAsync(jt.CoverUrl, ".jpg", cancellationToken);
                        if (!string.IsNullOrWhiteSpace(coverKey))
                        {
                            downloadedCovers[jt.CoverUrl] = coverKey;
                        }
                    }
                }

                if (!localArtistCache.TryGetValue(jt.ArtistId, out var artist))
                {
                    artist = await GetOrCreateArtistAsync(jt.ArtistName, jt.ArtistId, cancellationToken);
                    localArtistCache[jt.ArtistId] = artist;
                    hasNewPrerequisites = true;
                }

                var safeAlbumTitle = string.IsNullOrWhiteSpace(jt.AlbumName) ? "Singles" : jt.AlbumName;
                if (!localAlbumCache.TryGetValue(jt.AlbumId, out var album))
                {
                    album = await GetOrCreateAlbumAsync(safeAlbumTitle, jt.AlbumId, artist.Id, coverKey, cancellationToken);
                    localAlbumCache[jt.AlbumId] = album;
                    hasNewPrerequisites = true;
                }

                var trackGenreIds = new List<Guid>();
                if (jt.MusicInfo?.Tags?.Genres != null && jt.MusicInfo.Tags.Genres.Any())
                {
                    foreach (var genreName in jt.MusicInfo.Tags.Genres)
                    {
                        var formattedName = char.ToUpper(genreName[0]) + genreName.Substring(1).ToLower();
                        if (!localGenreCache.TryGetValue(formattedName, out var genreId))
                        {
                            var genre = await GetOrCreateGenreAsync(formattedName, cancellationToken);
                            genreId = genre.Id;
                            localGenreCache[formattedName] = genreId;
                            hasNewPrerequisites = true;
                        }
                        trackGenreIds.Add(genreId);
                    }
                }
                else
                {
                    if (!localGenreCache.TryGetValue("Unknown", out var genreId))
                    {
                        var fallbackGenre = await GetOrCreateGenreAsync("Unknown", cancellationToken);
                        genreId = fallbackGenre.Id;
                        localGenreCache["Unknown"] = genreId;
                        hasNewPrerequisites = true;
                    }
                    trackGenreIds.Add(genreId);
                }

                var trackMoodIds = new List<Guid>();
                if (jt.MusicInfo?.Tags?.Moods != null && jt.MusicInfo.Tags.Moods.Any())
                {
                    foreach (var moodName in jt.MusicInfo.Tags.Moods)
                    {
                        var formattedName = char.ToUpper(moodName[0]) + moodName.Substring(1).ToLower();
                        if (!localMoodCache.TryGetValue(formattedName, out var moodId))
                        {
                            var mood = await GetOrCreateMoodAsync(formattedName, cancellationToken);
                            moodId = mood.Id;
                            localMoodCache[formattedName] = moodId;
                            hasNewPrerequisites = true;
                        }
                        trackMoodIds.Add(moodId);
                    }
                }
                else
                {
                    if (!localMoodCache.TryGetValue("Various", out var moodId))
                    {
                        var fallbackMood = await GetOrCreateMoodAsync("Various", cancellationToken);
                        moodId = fallbackMood.Id;
                        localMoodCache["Various"] = moodId;
                        hasNewPrerequisites = true;
                    }
                    trackMoodIds.Add(moodId);
                }

                if (hasNewPrerequisites)
                {
                    await _context.SaveChangesAsync(cancellationToken);
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
                    await _context.SaveChangesAsync(cancellationToken);
                    syncedCount++;
                    continue;
                }

                if (!currentAlbumPositions.TryGetValue(album.Id, out int maxPos))
                {
                    maxPos = await _context.Tracks
                        .Where(t => t.AlbumId == album.Id)
                        .MaxAsync(t => (int?)t.AlbumPosition, cancellationToken) ?? 0;
                }

                int position = jt.Position > 0 ? jt.Position : maxPos + 1;
                if (position <= maxPos) position = maxPos + 1;

                currentAlbumPositions[album.Id] = position;
                
                var audioKey = await _jamendoApi.DownloadFileToTempAsync(jt.AudioDownloadUrl, ".mp3", cancellationToken);
                if (string.IsNullOrWhiteSpace(audioKey)) continue;

                var artistInputs = new List<TrackArtistDto> 
                { 
                    new TrackArtistDto(artist.Id, artist.Name, ArtistRole.Main) 
                };

                var createTrackCmd = new CreateTrackCommand(
                    album.Id, 
                    position, 
                    jt.Name, 
                    false, 
                    audioKey, 
                    coverKey, 
                    VisibilityStatus.Published,
                    artistInputs, 
                    trackGenreIds, 
                    trackMoodIds, 
                    jt.Isrc
                );

                var trackId = await _sender.Send(createTrackCmd, cancellationToken);
                var trackMapping = ExternalMapping.Create(trackId, nameof(Track), ExternalProvider.Jamendo, jt.Id);
                
                _context.ExternalMappings.Add(trackMapping);
                await _context.SaveChangesAsync(cancellationToken);

                syncedCount++;
                _logger.LogInformation("Track {TrackName} has been successfully sent for processing!", jt.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing Jamendo track:{Id}", jt.Id);
            }
        }

        return syncedCount;
    }

    private async Task<Genre> GetOrCreateGenreAsync(string name, CancellationToken cancellationToken)
    {
        var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
        if (genre == null)
        {
            genre = Genre.Create(name, null, _timeProvider.GetUtcNow().UtcDateTime); 
            _context.Genres.Add(genre);
        }
        return genre;
    }

    private async Task<Mood> GetOrCreateMoodAsync(string name, CancellationToken cancellationToken)
    {
        var mood = await _context.Moods.FirstOrDefaultAsync(m => m.Name == name, cancellationToken);
        if (mood == null)
        {
            mood = Mood.Create(name, null, _timeProvider.GetUtcNow().UtcDateTime);
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

        var artist = Artist.Create(null, name, "Imported from Jamendo", null, null, default, _timeProvider.GetUtcNow().UtcDateTime);
        _context.Artists.Add(artist);
        
        var newMapping = ExternalMapping.Create(artist.Id, nameof(Artist), ExternalProvider.Jamendo, jamendoId);
        _context.ExternalMappings.Add(newMapping);

        return artist;
    }

    private async Task<Album> GetOrCreateAlbumAsync(string title, string jamendoId, Guid artistId, string? coverKey, CancellationToken ct)
    {
        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Singles" : title;

        var mapping = await _context.ExternalMappings
            .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && m.ExternalId == jamendoId && m.EntityType == nameof(Album), ct);

        if (mapping != null)
            return await _context.Albums.FirstAsync(a => a.Id == mapping.InternalId, ct);

        var finalCoverUrl = !string.IsNullOrWhiteSpace(coverKey) && coverKey.StartsWith("temp/")
            ? coverKey.Replace("temp/", "covers/")
            : coverKey;

        var releaseType = string.Equals(safeTitle, "Singles", StringComparison.OrdinalIgnoreCase)
            ? ReleaseType.Single
            : ReleaseType.Album;

        var album = Album.Create(safeTitle, "Imported from Jamendo", finalCoverUrl, _timeProvider.GetUtcNow().UtcDateTime, releaseType, VisibilityStatus.Published, null, new List<Guid> { artistId }, _timeProvider.GetUtcNow().UtcDateTime);
        
        if (!string.IsNullOrWhiteSpace(coverKey) && coverKey.StartsWith("temp/"))
        {
            album.AddDomainEvent(new Yuviron.Domain.Events.TempFileNeedsMovingEvent(coverKey, "covers"));
        }

        _context.Albums.Add(album);
        
        var newMapping = ExternalMapping.Create(album.Id, nameof(Album), ExternalProvider.Jamendo, jamendoId);
        _context.ExternalMappings.Add(newMapping);

        return album;
    }
}
