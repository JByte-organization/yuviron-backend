using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Extensions;

public static class PermissionServiceExtensions
{
    public static Task<bool> CanStreamHighQualityAudioAsync(
        this IPermissionService service, 
        Guid userId, 
        CancellationToken ct = default)
    {
        return service.HasPermissionAsync(userId, AppPermission.PlayerHighQuality, ct);
    }
}
