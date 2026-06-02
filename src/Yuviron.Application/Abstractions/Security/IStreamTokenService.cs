using System;

namespace Yuviron.Application.Abstractions.Security;

public interface IStreamTokenService
{
    string GenerateToken(Guid trackId, int quality, DateTimeOffset expiration, Guid userId, string ipAddress);

    bool ValidateToken(Guid trackId, int quality, long expirationUnix, Guid userId, string ipAddress, string token);
}