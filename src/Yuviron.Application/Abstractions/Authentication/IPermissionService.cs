using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Authentication;

public interface IPermissionService
{
    Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<HashSet<string>> CachePermissionsAsync(User user, CancellationToken cancellationToken = default);
}