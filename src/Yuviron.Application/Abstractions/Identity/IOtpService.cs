
namespace Yuviron.Application.Abstractions.Authentication;

public interface IOtpService
{
    Task SaveLoginCodeHashAsync(string email, string codeHash, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<string?> GetLoginCodeHashAsync(string email, CancellationToken cancellationToken = default);
    Task RemoveLoginCodeAsync(string email, CancellationToken cancellationToken = default);
}