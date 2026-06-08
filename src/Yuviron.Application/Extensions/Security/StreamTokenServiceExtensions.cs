using Yuviron.Application.Abstractions.Security;

public static class StreamTokenServiceExtensions
{
    private const int TokenLifetimeHours = 6;

    public static string? GenerateAudioUrl(
        this IStreamTokenService streamTokenService,
        Guid trackId,
        int quality,
        string? fileKey,
        bool isAuthenticated,
        TimeProvider timeProvider,
        Guid userId)
    {
        if (!isAuthenticated || string.IsNullOrWhiteSpace(fileKey))
            return null;

        var expiration = timeProvider.GetUtcNow().AddHours(TokenLifetimeHours);
        var signature = streamTokenService.GenerateToken(trackId, quality, expiration, userId);
        var expUnix = expiration.ToUnixTimeSeconds();

        string fileName = fileKey.StartsWith("tracks/", StringComparison.OrdinalIgnoreCase) 
            ? "master.m3u8" 
            : Path.GetFileName(fileKey) ?? "audio_fallback";

        return $"/api/stream/tracks/{trackId}/{quality}/{fileName}?exp={expUnix}&uid={userId:N}&sig={signature}";
    }
}