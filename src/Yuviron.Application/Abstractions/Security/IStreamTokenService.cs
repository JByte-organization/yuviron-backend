using System;

namespace Yuviron.Application.Abstractions.Security;

public interface IStreamTokenService
{
    string GenerateToken(Guid trackId, int quality, DateTimeOffset expiration);

    bool ValidateToken(Guid trackId, int quality, long expirationUnix, string token);
}