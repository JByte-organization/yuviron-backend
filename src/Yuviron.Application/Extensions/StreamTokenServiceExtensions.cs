using System;
using System.IO;
using Yuviron.Application.Abstractions.Security; 

namespace Yuviron.Application.Extensions;

public static class StreamTokenServiceExtensions
{
    private const int TokenLifetimeHours = 6;

    /// <summary>
    /// Генерирует защищенную ссылку на стриминг аудио. Возвращает null для гостей или треков без файлов.
    /// </summary>
    public static string? GenerateAudioUrl(
        this IStreamTokenService streamTokenService, 
        Guid trackId, 
        string? fileKey, 
        bool isAuthenticated, 
        TimeProvider timeProvider)
    {
        if (!isAuthenticated || string.IsNullOrWhiteSpace(fileKey))
        {
            return null;
        }

        var expiration = timeProvider.GetUtcNow().AddHours(TokenLifetimeHours);
        
        var signature = streamTokenService.GenerateToken(trackId, expiration.UtcDateTime); 
        var fileName = Path.GetFileName(fileKey);
        var expUnix = expiration.ToUnixTimeSeconds();

        return $"/api/stream/tracks/{trackId}/{fileName}?exp={expUnix}&sig={signature}";
    }
}