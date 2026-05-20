using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

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

    // --- РЕАЛИЗАЦИЯ НАШЕГО ИДЕАЛЬНОГО МЕТОДА ---
    public async Task<bool> HasPermissionAsync(Guid userId, AppPermission permission, CancellationToken cancellationToken = default)
    {
        var permissions = await GetPermissionsAsync(userId, cancellationToken);
        return permissions.Contains(permission.ToString());
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

    public HashSet<string> CalculateUserPermissions(User user, DateTime utcNow)
    {
        var permissions = new HashSet<string>();

        if (user.AccountState == AccountState.Banned || user.AccountState == AccountState.Deleted)
        {
            return permissions; 
        }

        if (user.UserRoles != null)
        {
            foreach (var ur in user.UserRoles)
            {
                if (ur.Role?.RolePermissions != null)
                {
                    foreach (var rp in ur.Role.RolePermissions)
                    {
                        if (rp.Permission != null)
                        {
                            permissions.Add(rp.Permission.Name);
                        }
                    }
                }
            }
        }

        if (user.Subscriptions != null && user.HasActivePremiumSubscription(utcNow))
        {
            var premiumFlags = new[]
            {
                nameof(AppPermission.PlayerHighQuality),
                nameof(AppPermission.PlayerNoAds),
                nameof(AppPermission.PrivateSession), 
                nameof(AppPermission.CustomTheme),    
                nameof(AppPermission.AnimatedMedia)   
            };

            foreach (var perm in premiumFlags)
            {
                permissions.Add(perm);
            }
        }

        return permissions;
    }

    public async Task<HashSet<string>> CachePermissionsAsync(User user, CancellationToken cancellationToken = default)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var permissions = CalculateUserPermissions(user, utcNow);

        var expiration = (user.AccountState == AccountState.Banned || user.AccountState == AccountState.Deleted) 
            ? TimeSpan.FromMinutes(10) 
            : TimeSpan.FromHours(1);

        await _cacheService.SetAsync($"user:perms:{user.Id}", permissions, expiration, cancellationToken);

        return permissions;
    }
}