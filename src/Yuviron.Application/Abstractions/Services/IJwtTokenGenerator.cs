using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}