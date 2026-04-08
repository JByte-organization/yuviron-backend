using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Configuration; // <-- Изменили
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Infrastructure.Services.Audio;

public class HlsTranscodingService : IHlsTranscodingService
{
    private readonly string _storageRoot;
    private readonly ILogger<HlsTranscodingService> _logger;

    public HlsTranscodingService(IConfiguration configuration, ILogger<HlsTranscodingService> logger)
    {
        // ✅ Берем корень хранилища ТОЧНО ТАК ЖЕ, как в твоем LocalFileStorageService
        _storageRoot = configuration["FILE_STORAGE_ROOT"] 
                       ?? Environment.GetEnvironmentVariable("FILE_STORAGE_ROOT") 
                       ?? "/var/yuviron/storage";
        _storageRoot = Path.GetFullPath(_storageRoot);
        
        _logger = logger;
    }

    public async Task<string> TranscodeToHlsAsync(string inputStorageKey, string trackIdStr, CancellationToken cancellationToken = default)
    {
        // 1. Формируем абсолютные пути для FFmpeg
        var inputFilePath = Path.Combine(_storageRoot, inputStorageKey);
        
        // Папка для HLS-чанков трека
        var outputDirectory = Path.Combine(_storageRoot, "tracks", trackIdStr);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var playlistFilename = "master.m3u8";
        var playlistPath = Path.Combine(outputDirectory, playlistFilename);
        var segmentPattern = Path.Combine(outputDirectory, "segment_%03d.ts");

        _logger.LogInformation("Начинаем HLS конвертацию для трека {TrackId} из файла {File}", trackIdStr, inputFilePath);

        // 2. Запускаем FFMpeg с параметрами Spotify (-14 LUFS)
        await FFMpegArguments
            .FromFileInput(inputFilePath)
            .OutputToFile(playlistPath, overwrite: true, options => options
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(AudioQuality.Good) // ~192 kbps
                .WithCustomArgument("-af loudnorm=I=-14:LRA=11:TP=-1.5")
                .WithCustomArgument("-f hls")
                .WithCustomArgument("-hls_time 10")
                .WithCustomArgument("-hls_list_size 0")
                .WithCustomArgument($"-hls_segment_filename \"{segmentPattern}\"")
            )
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously();

        _logger.LogInformation("HLS нарезка завершена для трека {TrackId}", trackIdStr);

        // 3. Возвращаем относительный ключ (tracks/{id}/master.m3u8) для БД
        return Path.Combine("tracks", trackIdStr, playlistFilename).Replace("\\", "/");
    }
}