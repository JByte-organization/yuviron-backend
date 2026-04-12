using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Infrastructure.Services.Audio;

public class HlsTranscodingService : IHlsTranscodingService
{
    private readonly string _storageRoot;
    private readonly ILogger<HlsTranscodingService> _logger;

    public HlsTranscodingService(IConfiguration configuration, ILogger<HlsTranscodingService> logger)
    {
        _storageRoot = configuration["FILE_STORAGE_ROOT"] 
                       ?? Environment.GetEnvironmentVariable("FILE_STORAGE_ROOT") 
                       ?? "/var/yuviron/storage";
        _storageRoot = Path.GetFullPath(_storageRoot);
        
        _logger = logger;
    }

    public async Task<string> TranscodeToHlsAsync(string inputStorageKey, string trackIdStr, CancellationToken cancellationToken = default)
    {
        var inputFilePath = Path.Combine(_storageRoot, inputStorageKey);
        
        var outputDirectory = Path.Combine(_storageRoot, "tracks", trackIdStr);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var playlistFilename = "master.m3u8";
        var playlistPath = Path.Combine(outputDirectory, playlistFilename);
        
        var segmentPattern = Path.Combine(outputDirectory, "segment_%03d.ts").Replace("\\", "/");

        _logger.LogInformation("Starting HLS conversion for track {TrackId} from file {File}", trackIdStr, inputFilePath);

        await FFMpegArguments
            .FromFileInput(inputFilePath)
            .OutputToFile(playlistPath, overwrite: true, options => options
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(AudioQuality.Good) 
                .WithCustomArgument("-af loudnorm=I=-14:LRA=11:TP=-1.5")
                .WithCustomArgument("-f hls")
                .WithCustomArgument("-hls_time 10")
                .WithCustomArgument("-hls_list_size 0")
                .WithCustomArgument("-hls_segment_type mpegts") 
                .WithCustomArgument($"-hls_segment_filename \"{segmentPattern}\"")
            )
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously();

        _logger.LogInformation("HLS cutting completed for track {TrackId}", trackIdStr);

        return Path.Combine("tracks", trackIdStr, playlistFilename).Replace("\\", "/");
    }
}