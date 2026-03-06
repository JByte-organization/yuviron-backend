using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Infrastructure.Authentication;

public sealed class OtpService : IOtpService
{
    private readonly IDistributedCache _cache;
    private const string KeyPrefix = "login_code:";

    public OtpService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SaveLoginCodeHashAsync(string email, string codeHash, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        await _cache.SetStringAsync($"{KeyPrefix}{email}", codeHash, options, cancellationToken);
    }

    public async Task<string?> GetLoginCodeHashAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _cache.GetStringAsync($"{KeyPrefix}{email}", cancellationToken);
    }

    public async Task RemoveLoginCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync($"{KeyPrefix}{email}", cancellationToken);
    }
}