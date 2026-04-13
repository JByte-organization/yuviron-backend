
namespace Yuviron.Application.Abstractions.Services;

public interface IIdentityManager
{
    Task EnsureManagementRoleAsync(Guid userId, CancellationToken cancellationToken = default);
}