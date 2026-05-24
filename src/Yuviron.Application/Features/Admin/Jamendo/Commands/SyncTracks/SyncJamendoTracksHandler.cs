using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Services.Jamendo;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

public sealed class SyncJamendoTracksHandler : IRequestHandler<SyncJamendoTracksCommand, int>
{
    private readonly IJamendoApiService _jamendoApi;
    private readonly IJamendoEntityResolver _entityResolver;
    private readonly IJamendoMetadataResolver _metadataResolver;
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly ILogger<SyncJamendoTracksHandler> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public SyncJamendoTracksHandler(
        IJamendoApiService jamendoApi,
        IJamendoEntityResolver entityResolver,
        IJamendoMetadataResolver metadataResolver,
        IApplicationDbContext context,
        ISender sender,
        ILogger<SyncJamendoTracksHandler> logger,
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _jamendoApi = jamendoApi;
        _entityResolver = entityResolver;
        _metadataResolver = metadataResolver;
        _context = context;
        _sender = sender;
        _logger = logger;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(SyncJamendoTracksCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Only authenticated users can sync tracks.");
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        _logger.LogInformation("Starting syncing {Limit} tracks (Offset: {Offset})...", request.Limit, request.Offset);

        var jamendoTracks = await _jamendoApi.GetTracksAsync(request.Limit, request.Offset, cancellationToken);
        if (!jamendoTracks.Any()) return 0;

        var downloadedCovers = new Dictionary<string, Guid>(); 
        var currentAlbumPositions = new Dictionary<Guid, int>();
        int syncedCount = 0;

        foreach (var jt in jamendoTracks)
        {
            try
            {
                if (await IsTrackAlreadySyncedAsync(jt.Id, jt.Isrc, jt.Name, cancellationToken))
                {
                    continue;
                }

                Guid? coverFileId = await GetOrCreateCoverAsync(jt.CoverUrl, adminId, utcNow, downloadedCovers, cancellationToken);

                var artist = await _entityResolver.ResolveArtistAsync(jt.ArtistName, jt.ArtistId, cancellationToken);
                var album = await _entityResolver.ResolveAlbumAsync(jt.AlbumName, jt.AlbumId, artist.Id, coverFileId, adminId, cancellationToken);
                var genreIds = await _metadataResolver.ResolveGenresAsync(jt.MusicInfo?.Tags?.Genres, cancellationToken);
                var moodIds = await _metadataResolver.ResolveMoodsAsync(jt.MusicInfo?.Tags?.Moods, cancellationToken);

                if (!currentAlbumPositions.TryGetValue(album.Id, out int nextPos))
                {
                    nextPos = await _entityResolver.GetNextAlbumPositionAsync(album.Id, cancellationToken);
                }
                int position = jt.Position > 0 && jt.Position >= nextPos ? jt.Position : nextPos;
                currentAlbumPositions[album.Id] = position + 1;

                var audioDownloaded = await _jamendoApi.DownloadFileToTempAsync(jt.AudioDownloadUrl, ".mp3", cancellationToken);
                if (audioDownloaded == null) continue;

                var audioMeta = FileMetadata.Create(
                    audioDownloaded.FileId, adminId, "jamendo_track.mp3", audioDownloaded.ContentType, audioDownloaded.SizeBytes, audioDownloaded.TempKey, utcNow);
                _context.FileMetadata.Add(audioMeta);
                await _context.SaveChangesAsync(cancellationToken);

                var createTrackCmd = new CreateTrackCommand(
                    album.Id, 
                    position, 
                    jt.Name, 
                    false, 
                    audioDownloaded.FileId, 
                    coverFileId,          
                    VisibilityStatus.Published,
                    new List<TrackArtistDto> { new TrackArtistDto(artist.Id, artist.Name, ArtistRole.Main) },
                    genreIds, 
                    moodIds, 
                    jt.Isrc
                );

                var trackId = await _sender.Send(createTrackCmd, cancellationToken);
                
                _context.ExternalMappings.Add(ExternalMapping.Create(trackId, nameof(Track), ExternalProvider.Jamendo, jt.Id));
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

    private async Task<bool> IsTrackAlreadySyncedAsync(string jamendoId, string? isrc, string trackName, CancellationToken ct)
    {
        var existingMapping = await _context.ExternalMappings
            .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && m.ExternalId == jamendoId && m.EntityType == nameof(Track), ct);

        if (existingMapping != null)
        {
            var mappedTrack = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == existingMapping.InternalId, ct);
            if (mappedTrack == null || mappedTrack.IsDeleted)
            {
                _context.ExternalMappings.Remove(existingMapping);
                await _context.SaveChangesAsync(ct);
                _logger.LogWarning("Dead mapping found and removed. Downloading track {TrackName} again.", trackName);
                return false;
            }
            
            _logger.LogInformation("Jamendo track:{Id} is already downloaded and healthy. Skip.", jamendoId);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(isrc))
        {
            var trackByIsrc = await _context.Tracks.FirstOrDefaultAsync(t => t.Isrc == isrc, ct);
            if (trackByIsrc != null)
            {
                _context.ExternalMappings.Add(ExternalMapping.Create(trackByIsrc.Id, nameof(Track), ExternalProvider.Jamendo, jamendoId));
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Track {Title} found via ISRC. Let's connect.", trackName);
                return true;
            }
        }

        return false;
    }

    private async Task<Guid?> GetOrCreateCoverAsync(string? coverUrl, Guid adminId, DateTime utcNow, Dictionary<string, Guid> cache, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(coverUrl)) return null;
        if (cache.TryGetValue(coverUrl, out var existingFileId)) return existingFileId;

        var downloaded = await _jamendoApi.DownloadFileToTempAsync(coverUrl, ".jpg", ct);
        if (downloaded == null) return null;

        var fileMeta = FileMetadata.Create(
            downloaded.FileId, adminId, "jamendo_cover.jpg", downloaded.ContentType, downloaded.SizeBytes, downloaded.TempKey, utcNow);
        
        _context.FileMetadata.Add(fileMeta);
        cache[coverUrl] = downloaded.FileId;
        
        return downloaded.FileId;
    }
}