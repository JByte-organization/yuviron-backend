namespace Yuviron.Application.Abstractions.Authentication;

public interface IOtpService
{
    Task SaveLoginCodeHashAsync(string email, string codeHash, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<string?> GetLoginCodeHashAsync(string email, CancellationToken cancellationToken = default);
    Task RemoveLoginCodeAsync(string email, CancellationToken cancellationToken = default);
    
    Task SaveAdminLoginCodeHashAsync(string email, string codeHash, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<string?> GetAdminLoginCodeHashAsync(string email, CancellationToken cancellationToken = default);
    Task RemoveAdminLoginCodeAsync(string email, CancellationToken cancellationToken = default);
    
    Task SaveConfirmationTokenAsync(string token, string email, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<string?> GetEmailByConfirmationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RemoveConfirmationTokenAsync(string token, CancellationToken cancellationToken = default);
    
    Task SavePasswordResetTokenAsync(string token, string email, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<string?> GetEmailByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RemovePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
}