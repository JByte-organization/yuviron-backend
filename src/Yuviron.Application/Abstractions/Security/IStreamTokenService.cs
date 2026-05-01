using System;

namespace Yuviron.Application.Abstractions.Security;

public interface IStreamTokenService
{
    string GenerateToken(Guid trackId, DateTimeOffset expiration);

    bool ValidateToken(Guid trackId, long expirationUnix, string token);
}