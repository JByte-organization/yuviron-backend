using Microsoft.Extensions.Caching.Distributed;
using Yuviron.Application.Abstractions.Authentication;

namespace Yuviron.Infrastructure.Authentication;

public sealed class OtpService : IOtpService
{
    private readonly IDistributedCache _cache;
    private const string LoginKeyPrefix = "login_code:";
    private const string ConfirmationKeyPrefix = "email_confirm:"; 
    private const string PasswordResetKeyPrefix = "pwd_reset:";

    public OtpService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SaveLoginCodeHashAsync(string email, string codeHash, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };
        await _cache.SetStringAsync($"{LoginKeyPrefix}{email}", codeHash, options, cancellationToken);
    }

    public async Task<string?> GetLoginCodeHashAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _cache.GetStringAsync($"{LoginKeyPrefix}{email}", cancellationToken);
    }

    public async Task RemoveLoginCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync($"{LoginKeyPrefix}{email}", cancellationToken);
    }

    public async Task SaveConfirmationTokenAsync(string token, string email, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };
        await _cache.SetStringAsync($"{ConfirmationKeyPrefix}{token}", email, options, cancellationToken);
    }

    public async Task<string?> GetEmailByConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _cache.GetStringAsync($"{ConfirmationKeyPrefix}{token}", cancellationToken);
    }

    public async Task RemoveConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync($"{ConfirmationKeyPrefix}{token}", cancellationToken);
    }
    
    public async Task SavePasswordResetTokenAsync(string token, string email, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };
        await _cache.SetStringAsync($"{PasswordResetKeyPrefix}{token}", email, options, cancellationToken);
    }

    public async Task<string?> GetEmailByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _cache.GetStringAsync($"{PasswordResetKeyPrefix}{token}", cancellationToken);
    }

    public async Task RemovePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync($"{PasswordResetKeyPrefix}{token}", cancellationToken);
    }
}