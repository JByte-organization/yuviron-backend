using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Events;

namespace Yuviron.MediaWorker.Consumers;

public class AudioTranscodingConsumer : IConsumer<AudioNeedsTranscodingEvent>
{
    private readonly IHlsTranscodingService _hlsService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AudioTranscodingConsumer> _logger;
    private readonly TimeProvider _timeProvider;

    public AudioTranscodingConsumer(
        IHlsTranscodingService hlsService,
        IFileStorageService fileStorageService,
        IApplicationDbContext context,
        ILogger<AudioTranscodingConsumer> logger,
        TimeProvider timeProvider)
    {
        _hlsService = hlsService;
        _fileStorageService = fileStorageService;
        _context = context;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<AudioNeedsTranscodingEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("RabbitMQ: Starting HLS slicing for the track: {TrackId}", message.TrackId);

        try
        {
            string hlsUrl = await _hlsService.TranscodeToHlsAsync(
                message.TempAudioStorageKey,
                message.TrackId.ToString(),
                context.CancellationToken);

            string finalAudioKey = await _fileStorageService.MoveAsync(
                message.TempAudioStorageKey,
                $"tracks/{message.TrackId}",
                context.CancellationToken);

            var track = await _context.Tracks
                .FirstOrDefaultAsync(t => t.Id == message.TrackId, context.CancellationToken);

            if (track != null)
            {
                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

                track.MarkAsReady(hlsUrl, finalAudioKey, utcNow);

                await _context.SaveChangesAsync(context.CancellationToken);

                _logger.LogInformation("Track {TrackId} was processed successfully. Master file and HLS saved.", message.TrackId);
            }
            else
            {
                _logger.LogWarning("Track {TrackId} is cut, but not found in the database!", message.TrackId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FFmpeg error for track {TrackId}", message.TrackId);

            var track = await _context.Tracks
                .FirstOrDefaultAsync(t => t.Id == message.TrackId, context.CancellationToken);

            if (track != null)
            {
                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
                var failedDirectoryPath = $"tracks/{message.TrackId}"; 

                track.MarkAsFailed(utcNow, failedDirectoryPath);
                await _context.SaveChangesAsync(context.CancellationToken);
            }

            throw;
        }
    }
}