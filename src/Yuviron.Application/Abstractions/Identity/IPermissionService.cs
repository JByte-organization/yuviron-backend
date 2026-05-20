using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Abstractions.Authentication;

public interface IPermissionService
{
    Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<HashSet<string>> CachePermissionsAsync(User user, CancellationToken cancellationToken = default);
    
    HashSet<string> CalculateUserPermissions(User user, DateTime utcNow);

    Task InvalidatePermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    // --- ТОТ САМЫЙ МЕТОД ---
    Task<bool> HasPermissionAsync(Guid userId, AppPermission permission, CancellationToken cancellationToken = default);
}