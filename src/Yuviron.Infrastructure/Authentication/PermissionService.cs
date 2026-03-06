using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using System; // Добавлено для TimeProvider

namespace Yuviron.Infrastructure.Identity;

public class PermissionService : IPermissionService
{
    private readonly ICacheService _cacheService;
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public PermissionService(
        ICacheService cacheService,
        IApplicationDbContext context,
        TimeProvider timeProvider)
    {
        _cacheService = cacheService;
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string key = $"user:perms:{userId}";
        var cachedPerms = await _cacheService.GetAsync<HashSet<string>>(key, cancellationToken);

        if (cachedPerms != null) return cachedPerms;

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.Subscriptions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null) return new HashSet<string>();

        return await CachePermissionsAsync(user, cancellationToken);
    }

    public async Task InvalidatePermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string key = $"user:perms:{userId}";
        await _cacheService.RemoveAsync(key, cancellationToken);
    }

    public async Task<HashSet<string>> CachePermissionsAsync(User user, CancellationToken cancellationToken = default)
    {
        var permissions = new HashSet<string>();

        if (user.AccountState == AccountState.Banned || user.AccountState == AccountState.Deleted)
        {
            await _cacheService.SetAsync($"user:perms:{user.Id}", permissions, TimeSpan.FromMinutes(10), cancellationToken);
            return permissions;
        }

        if (user.UserRoles != null)
        {
            foreach (var role in user.UserRoles.Select(ur => ur.Role))
            {
                if (role.RolePermissions != null)
                {
                    foreach (var rp in role.RolePermissions)
                    {
                        permissions.Add(rp.Permission.Name);
                    }
                }
            }
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        if (user.Subscriptions != null && user.HasActivePremiumSubscription(utcNow))
        {
            var premiumFlags = new[]
            {
                nameof(AppPermission.PlayerHighQuality),
                nameof(AppPermission.PlayerNoAds),
            };

            foreach (var perm in premiumFlags)
            {
                permissions.Add(perm);
            }
        }

        await _cacheService.SetAsync(
            $"user:perms:{user.Id}",
            permissions,
            TimeSpan.FromHours(1),
            cancellationToken);

        return permissions;
    }
}