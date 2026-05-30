using System;
using System.IO;
using Yuviron.Application.Abstractions.Security; 

namespace Yuviron.Application.Extensions;

public static class StreamTokenServiceExtensions
{
    private const int TokenLifetimeHours = 6;

    public static string? GenerateAudioUrl(
        this IStreamTokenService streamTokenService, 
        Guid trackId, 
        int quality, 
        string? fileKey, 
        bool isAuthenticated, 
        TimeProvider timeProvider)
    {
        if (!isAuthenticated || string.IsNullOrWhiteSpace(fileKey))
        {
            return null;
        }

        var expiration = timeProvider.GetUtcNow().AddHours(TokenLifetimeHours);
        var signature = streamTokenService.GenerateToken(trackId, quality, expiration); 
        var expUnix = expiration.ToUnixTimeSeconds();

        string fileName;

        if (fileKey.StartsWith("tracks/", StringComparison.OrdinalIgnoreCase))
        {
            fileName = "master.m3u8";
        }
        else
        {
            fileName = Path.GetFileName(fileKey);
            
            if (string.IsNullOrWhiteSpace(fileName)) 
            {
                fileName = "audio_fallback"; 
            }
        }

        return $"/api/stream/tracks/{trackId}/{quality}/{fileName}?exp={expUnix}&sig={signature}";
    }
}
