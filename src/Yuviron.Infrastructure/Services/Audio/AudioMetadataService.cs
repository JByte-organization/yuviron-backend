using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Infrastructure.Services.Audio;

public sealed class AudioMetadataService : IAudioMetadataService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AudioMetadataService> _logger;

    public AudioMetadataService(
        IFileStorageService fileStorageService, 
        ILogger<AudioMetadataService> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<AudioMetadata> GetAudioMetadataAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        await using var stream = await _fileStorageService.GetFileStreamAsync(storageKey, cancellationToken);
        
        if (stream == null || stream.Length == 0)
        {
            throw new InvalidOperationException($"Audio file not found or empty at key: {storageKey}");
        }

        try
        {
            var fileAbstraction = new StreamFileAbstraction(storageKey, stream);
            
            using var tfile = TagLib.File.Create(fileAbstraction);

            var properties = tfile.Properties;
            if (properties == null || properties.Duration == TimeSpan.Zero)
            {
                throw new InvalidOperationException("Failed to extract duration. The file might be corrupted or not an audio file.");
            }

            if (!properties.MediaTypes.HasFlag(TagLib.MediaTypes.Audio))
            {
                throw new InvalidOperationException("The uploaded file is not a valid audio format.");
            }

            return new AudioMetadata(
                DurationMs: (int)properties.Duration.TotalMilliseconds,
                MimeType: tfile.MimeType ?? "audio/unknown",
                Bitrate: properties.AudioBitrate,
                Codec: properties.Description ?? "Unknown Codec"
            );
        }
        catch (TagLib.UnsupportedFormatException ex)
        {
            _logger.LogWarning(ex, "Attempted to read metadata from an unsupported file format: {StorageKey}", storageKey);
            throw new InvalidOperationException("The audio format is not supported.");
        }
        catch (TagLib.CorruptFileException ex)
        {
            _logger.LogWarning(ex, "Attempted to read a corrupted audio file: {StorageKey}", storageKey);
            throw new InvalidOperationException("The audio file is corrupted.");
        }
    }
}