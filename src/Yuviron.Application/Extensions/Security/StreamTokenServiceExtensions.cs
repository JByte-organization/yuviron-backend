using Yuviron.Application.Abstractions.Security;

public static class StreamTokenServiceExtensions
{
    // Short-lived on purpose: the link itself can't be bound to a stable client
    // identifier (IP/JTI churn breaks legitimate playback), so anti-sharing relies
    // on a small exposure window plus background anomaly detection (see GetAudioStreamHandler).
    private const int TokenLifetimeMinutes = 60;

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

        var expiration = timeProvider.GetUtcNow().AddMinutes(TokenLifetimeMinutes);
        var signature = streamTokenService.GenerateToken(trackId, quality, expiration, userId);
        var expUnix = expiration.ToUnixTimeSeconds();

        string fileName = fileKey.StartsWith("tracks/", StringComparison.OrdinalIgnoreCase) 
            ? "master.m3u8" 
            : Path.GetFileName(fileKey) ?? "audio_fallback";

        return $"/api/stream/tracks/{trackId}/{quality}/{fileName}?exp={expUnix}&uid={userId:N}&sig={signature}";
    }
}