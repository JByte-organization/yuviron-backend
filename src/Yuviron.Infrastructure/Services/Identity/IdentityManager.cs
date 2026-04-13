using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Infrastructure.Identity;

public sealed class IdentityManager : IIdentityManager
{
    private readonly IApplicationDbContext _context;

    public IdentityManager(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureManagementRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
                       .Include(u => u.UserRoles)
                       .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), userId);

        var managementRoleStr = nameof(RoleName.ManagementUser);
        var managementRole = await _context.Roles
                                 .FirstOrDefaultAsync(r => r.Name == managementRoleStr, cancellationToken)
                             ?? throw new InvalidOperationException($"Role '{managementRoleStr}' not found.");

        var currentRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        
        if (!currentRoleIds.Contains(managementRole.Id))
        {
            currentRoleIds.Add(managementRole.Id);
            
            user.SyncRoles(currentRoleIds);
            user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id));
        }
        
    }
}