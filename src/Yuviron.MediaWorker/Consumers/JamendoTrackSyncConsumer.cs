using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Services.Jamendo;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events.External;

namespace Yuviron.MediaWorker.Consumers;

public class JamendoTrackSyncConsumer : IConsumer<JamendoTrackSyncRequestedEvent>
{
    private readonly IJamendoApiService _jamendoApi;
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly ILogger<JamendoTrackSyncConsumer> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IJamendoMetadataResolver _metadataResolver;
    private readonly IAudioMetadataService _audioMetadataService;

    public JamendoTrackSyncConsumer(
        IJamendoApiService jamendoApi,
        ICatalogContext catalogContext, ISystemContext systemContext,
        ILogger<JamendoTrackSyncConsumer> logger,
        TimeProvider timeProvider,
        IJamendoMetadataResolver metadataResolver,
        IAudioMetadataService audioMetadataService)
    {
        _jamendoApi = jamendoApi;
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _logger = logger;
        _timeProvider = timeProvider;
        _metadataResolver = metadataResolver;
        _audioMetadataService = audioMetadataService;
    }

    public async Task Consume(ConsumeContext<JamendoTrackSyncRequestedEvent> catalogContext)
    {
        var message = catalogContext.Message;
        _logger.LogInformation("Starting background sync for Jamendo track: {TrackName} ({JamendoId})", message.TrackName, message.JamendoId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // 1. Download audio
        var audioDownloaded = await _jamendoApi.DownloadFileToTempAsync(message.AudioDownloadUrl!, ".mp3", catalogContext.CancellationToken);
        if (audioDownloaded == null)
        {
            throw new Exception($"Failed to download audio from Jamendo for track {message.JamendoId}.");
        }

        var audioMeta = FileMetadata.Create(
            audioDownloaded.FileId, 
            message.AdminId, 
            "jamendo_track.mp3", 
            audioDownloaded.ContentType, 
            audioDownloaded.SizeBytes, 
            audioDownloaded.TempKey, 
            utcNow);
        
        _systemContext.Add(audioMeta);
        await _catalogContext.SaveChangesAsync(catalogContext.CancellationToken);

        // 2. Download cover (optional)
        Guid? coverFileId = null;
        if (!string.IsNullOrWhiteSpace(message.CoverUrl))
        {
            var coverDownloaded = await _jamendoApi.DownloadFileToTempAsync(message.CoverUrl, ".jpg", catalogContext.CancellationToken);
            if (coverDownloaded != null)
            {
                coverFileId = coverDownloaded.FileId;
                var coverMeta = FileMetadata.Create(
                    coverFileId.Value, 
                    message.AdminId, 
                    "jamendo_cover.jpg", 
                    coverDownloaded.ContentType, 
                    coverDownloaded.SizeBytes, 
                    coverDownloaded.TempKey, 
                    utcNow);
                
                _systemContext.Add(coverMeta);
                await _catalogContext.SaveChangesAsync(catalogContext.CancellationToken);
            }
        }

        // 3. Resolve metadata
        var genreIds = await _metadataResolver.ResolveGenresAsync(message.Genres, catalogContext.CancellationToken);
        var moodIds = await _metadataResolver.ResolveMoodsAsync(message.Moods, catalogContext.CancellationToken);

        // 4. Create Track (Logic extracted from CreateTrackHandler to bypass AuthorizationBehavior)
        var trackId = Guid.NewGuid();
        var targetAudioFolder = $"tracks/{trackId}";

        var audioClaim = await _systemContext.ClaimFileAsync(
            audioDownloaded.FileId, message.AdminId, "audio/", targetAudioFolder, catalogContext.CancellationToken);

        ClaimedFileResult? coverClaim = null;
        if (coverFileId.HasValue)
        {
            coverClaim = await _systemContext.ClaimFileAsync(
                coverFileId.Value, message.AdminId, "image/", "covers", catalogContext.CancellationToken);
        }

        var audioMetadata = await _audioMetadataService.GetAudioMetadataAsync(audioClaim.SourceKey, catalogContext.CancellationToken);

        var track = Track.Create(
            trackId, 
            message.AlbumId,
            message.Position,
            message.TrackName,
            audioMetadata.DurationMs,
            false,
            coverClaim?.FinalPath,   
            audioClaim.SourceKey,
            VisibilityStatus.Draft,
            message.Isrc,
            new List<(Guid ArtistId, ArtistRole Role)> { (message.ArtistId, ArtistRole.Main) },
            genreIds, 
            moodIds, 
            utcNow);

        if (coverClaim != null)
        {
            track.RegisterFileSwapEvents(coverClaim);
        }

        _catalogContext.Add(track);
        
        // 5. External Mapping
        _systemContext.Add(ExternalMapping.Create(trackId, nameof(Track), ExternalProvider.Jamendo, message.JamendoId));
        
        await _catalogContext.SaveChangesAsync(catalogContext.CancellationToken);

        _logger.LogInformation("Successfully synced Jamendo track: {TrackName}", message.TrackName);
    }
}
