using System.Security.Cryptography;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Infrastructure.Utilities;

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
        var inputFilePath = StoragePathValidator.GetValidatedFullPath(_storageRoot, inputStorageKey);
        var outputDirectory = StoragePathValidator.GetValidatedFullPath(_storageRoot, Path.Combine("tracks", trackIdStr));
        
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var keyFileName = "encryption.key";
        var keyFilePath = Path.Combine(outputDirectory, keyFileName);
        var keyBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }
        await File.WriteAllBytesAsync(keyFilePath, keyBytes, cancellationToken);

        var keyInfoFileName = "key_info.txt";
        var keyInfoFilePath = Path.Combine(outputDirectory, keyInfoFileName);
        
        var playerKeyUrl = $"/api/stream/tracks/{trackIdStr}/key"; 
        
        var keyInfoContent = $"{playerKeyUrl}\n{keyFilePath.Replace("\\", "/")}\n";
        await File.WriteAllTextAsync(keyInfoFilePath, keyInfoContent, cancellationToken);

        var playlistFilename = "master.m3u8";
        var playlistPath = Path.Combine(outputDirectory, playlistFilename);
        var segmentPattern = Path.Combine(outputDirectory, "segment_%03d.ts").Replace("\\", "/");

        _logger.LogInformation("Starting encrypted HLS conversion for track {TrackId}", trackIdStr);

        await FFMpegArguments
            .FromFileInput(inputFilePath)
            .OutputToFile(playlistPath, overwrite: true, options => options
                .WithCustomArgument("-vn")
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(AudioQuality.Good) 
                .WithCustomArgument("-af loudnorm=I=-14:LRA=11:TP=-1.5")
                .WithCustomArgument("-f hls")
                .WithCustomArgument("-hls_time 10")
                .WithCustomArgument("-hls_list_size 0")
                .WithCustomArgument("-hls_segment_type mpegts") 
                .WithCustomArgument($"-hls_key_info_file \"{keyInfoFilePath.Replace("\\", "/")}\"") 
                .WithCustomArgument($"-hls_segment_filename \"{segmentPattern}\"")
            )
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously();

        if (File.Exists(keyInfoFilePath))
        {
            File.Delete(keyInfoFilePath);
        }

        _logger.LogInformation("Encrypted HLS cutting completed for track {TrackId}", trackIdStr);

        return Path.Combine("tracks", trackIdStr, playlistFilename).Replace("\\", "/");
    }
}