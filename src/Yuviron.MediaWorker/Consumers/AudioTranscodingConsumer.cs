using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Admin.Tracks.Commands.MarkTrackAsTranscoded;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.MediaWorker.Consumers;

public class AudioTranscodingConsumer : IConsumer<AudioNeedsTranscodingEvent>
{
    private readonly IHlsTranscodingService _hlsService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICatalogContext _catalogContext;
    private readonly ISender _sender; 
    private readonly ILogger<AudioTranscodingConsumer> _logger;

    public AudioTranscodingConsumer(
        IHlsTranscodingService hlsService,
        IFileStorageService fileStorageService,
        ICatalogContext catalogContext,
        ISender sender,
        ILogger<AudioTranscodingConsumer> logger)
    {
        _hlsService = hlsService;
        _fileStorageService = fileStorageService;
        _catalogContext = catalogContext;
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AudioNeedsTranscodingEvent> catalogContext)
    {
        var message = catalogContext.Message;
        _logger.LogInformation("RabbitMQ: Starting HLS slicing for the track: {TrackId}", message.TrackId);

        var isTrackValid = await _catalogContext.Tracks
            .AsNoTracking()
            .AnyAsync(t => t.Id == message.TrackId &&
                           t.ProcessingStatus == TrackProcessingStatus.Processing &&
                           t.AudioStorageKey == message.TempAudioStorageKey,
                           catalogContext.CancellationToken);

        if (!isTrackValid)
        {
            _logger.LogInformation("Skipping stale transcoding job for track {TrackId}.", message.TrackId);
            return;
        }

        try
        {
            string hlsUrl = await _hlsService.TranscodeToHlsAsync(
                message.TempAudioStorageKey,
                message.TrackId.ToString(),
                catalogContext.CancellationToken);

            string finalAudioKey = await _fileStorageService.MoveAsync(
                message.TempAudioStorageKey,
                $"tracks/{message.TrackId}",
                catalogContext.CancellationToken);

            await _sender.Send(new MarkTrackAsTranscodedCommand(
                message.TrackId,
                message.TempAudioStorageKey,
                hlsUrl,
                finalAudioKey
            ), catalogContext.CancellationToken);

            _logger.LogInformation("Track {TrackId} processing completed and command sent to App layer.", message.TrackId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FFmpeg error for track {TrackId}. Throwing to allow MassTransit Retry.", message.TrackId);
            throw;
        }
    }
}