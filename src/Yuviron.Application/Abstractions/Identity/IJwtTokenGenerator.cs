using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, string sessionType = "client");

    string GenerateRefreshToken();
    string HashRefreshToken(string token);
}