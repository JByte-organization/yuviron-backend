using System.Security.Cryptography;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; 
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration; 
using Yuviron.Infrastructure.Utilities;

namespace Yuviron.Infrastructure.Services.Audio;

public class HlsTranscodingService : IHlsTranscodingService
{
    private readonly string _storageRoot;
    private readonly ILogger<HlsTranscodingService> _logger;
    private readonly AudioSettingsOptions _audioSettings;

    public HlsTranscodingService(
        IOptions<StorageOptions> storageOptions, 
        IOptions<AudioSettingsOptions> audioOptions, 
        ILogger<HlsTranscodingService> logger)
    {
        var configuredRoot = Environment.GetEnvironmentVariable("FILE_STORAGE_ROOT") ?? storageOptions.Value.RootPath;
        _storageRoot = Path.GetFullPath(configuredRoot);
        _audioSettings = audioOptions.Value;
        _logger = logger;
    }

    public async Task<string> TranscodeToHlsAsync(string inputStorageKey, string trackIdStr, CancellationToken cancellationToken = default)
    {
        var inputFilePath = StoragePathValidator.GetValidatedFullPath(_storageRoot, inputStorageKey);
        var finalTrackDirectory = StoragePathValidator.GetValidatedFullPath(_storageRoot, Path.Combine("tracks", trackIdStr));
        
        var tempTranscodeDir = Path.Combine(_storageRoot, "temp", $"transcode_{trackIdStr}_{Guid.NewGuid():N}");

        try
        {
            _logger.LogInformation("Starting dual-quality encrypted HLS conversion for track {TrackId}", trackIdStr);

            foreach (var quality in _audioSettings.HlsQualities)
            {
                var qualityTempDir = Path.Combine(tempTranscodeDir, quality.ToString());
                Directory.CreateDirectory(qualityTempDir);

                var keyFileName = "encryption.key";
                var keyFilePath = Path.Combine(qualityTempDir, keyFileName);
                var keyBytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(keyBytes);
                }
                await File.WriteAllBytesAsync(keyFilePath, keyBytes, cancellationToken);

                var keyInfoFileName = "key_info.txt";
                var keyInfoFilePath = Path.Combine(qualityTempDir, keyInfoFileName);
                var playerKeyUrl = $"/api/stream/tracks/{trackIdStr}/{quality}/key"; 
                var keyInfoContent = $"{playerKeyUrl}\n{keyFilePath.Replace("\\", "/")}\n";
                
                try
                {
                    await File.WriteAllTextAsync(keyInfoFilePath, keyInfoContent, cancellationToken);

                    var playlistPath = Path.Combine(qualityTempDir, "master.m3u8");
                    var segmentPattern = Path.Combine(qualityTempDir, "segment_%03d.ts").Replace("\\", "/");

                    await FFMpegArguments
                        .FromFileInput(inputFilePath)
                        .OutputToFile(playlistPath, overwrite: true, options => options
                            .WithCustomArgument("-vn")
                            .WithAudioCodec(AudioCodec.Aac)
                            .WithCustomArgument($"-b:a {quality}k") 
                            .WithCustomArgument(_audioSettings.FfmpegAudioFilters)
                            .WithCustomArgument("-f hls")
                            .WithCustomArgument("-hls_time 10")
                            .WithCustomArgument("-hls_list_size 0")
                            .WithCustomArgument("-hls_segment_type mpegts") 
                            .WithCustomArgument($"-hls_key_info_file \"{keyInfoFilePath.Replace("\\", "/")}\"") 
                            .WithCustomArgument($"-hls_segment_filename \"{segmentPattern}\"")
                        )
                        .CancellableThrough(cancellationToken)
                        .ProcessAsynchronously();
                }
                finally
                {
                    if (File.Exists(keyInfoFilePath))
                    {
                        File.Delete(keyInfoFilePath);
                    }
                }
            }

            if (Directory.Exists(finalTrackDirectory))
            {
                Directory.Delete(finalTrackDirectory, true); 
            }
            Directory.Move(tempTranscodeDir, finalTrackDirectory);

            _logger.LogInformation("Dual-quality encrypted HLS cutting completed for track {TrackId}", trackIdStr);

            return Path.Combine("tracks", trackIdStr).Replace("\\", "/");
        }
        finally
        {
            if (Directory.Exists(tempTranscodeDir))
            {
                try { Directory.Delete(tempTranscodeDir, true); } catch { /* Игнорируем ошибки при очистке */ }
            }
        }
    }
}