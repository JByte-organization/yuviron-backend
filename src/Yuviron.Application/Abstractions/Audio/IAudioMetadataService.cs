using System;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.Services;

public record AudioMetadata(
    int DurationMs, 
    string MimeType, 
    int Bitrate, 
    string Codec
);

public interface IAudioMetadataService
{
    Task<AudioMetadata> GetAudioMetadataAsync(string storageKey, CancellationToken cancellationToken = default);
}