using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Yuviron.Application.Abstractions.Security;

namespace Yuviron.Infrastructure.Services.Security;

public sealed class StreamTokenService : IStreamTokenService
{
    private readonly byte[] _secretKey;

    public StreamTokenService(IConfiguration configuration)
    {
        var keyString = configuration["StreamSecurity:SecretKey"];
        if (string.IsNullOrWhiteSpace(keyString))
        {
            throw new InvalidOperationException("StreamSecurity:SecretKey is missing in configuration.");
        }
        
        _secretKey = Encoding.UTF8.GetBytes(keyString);
    }

    public string GenerateToken(Guid trackId, int quality, DateTimeOffset expiration, Guid userId)
    {
        var expirationUnix = expiration.ToUnixTimeSeconds();

        var payload = $"{trackId:N}:{quality}:{expirationUnix}:{userId:N}";

        using var hmac = new HMACSHA256(_secretKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        return Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public bool ValidateToken(Guid trackId, int quality, long expirationUnix, Guid userId, string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expirationUnix);
        if (DateTimeOffset.UtcNow > expirationTime) return false;

        var payload = $"{trackId:N}:{quality}:{expirationUnix}:{userId:N}";
        
        using var hmac = new HMACSHA256(_secretKey);
        var expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expectedToken = Convert.ToBase64String(expectedHash).Replace("+", "-").Replace("/", "_").TrimEnd('=');

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(token), 
            Encoding.UTF8.GetBytes(expectedToken));
    }
}