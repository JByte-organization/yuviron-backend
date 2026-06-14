using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Extensions;

public static class UserAccountExtensions
{
    /// <summary>
    /// Проверяет состояние аккаунта. Выбрасывает UnauthorizedAccessException, если вход запрещен.
    /// Автоматически снимает бан, если его срок истек.
    /// </summary>
    public static async Task EnsureAllowedToLoginAsync(
        this User user, 
        IIdentityContext identityContext, 
        DateTime utcNow, 
        CancellationToken cancellationToken)
    {
        if (user.AccountState == AccountState.Deleted)
        {
            throw new UnauthorizedAccessException("This account has been deleted.");
        }

        if (user.AccountState == AccountState.Banned)
        {
            var activeBlocks = await identityContext.UserBlocks
                .Where(b => b.UserId == user.Id && b.IsActive)
                .ToListAsync(cancellationToken);

            bool isStillBanned = activeBlocks.Any(b => b.EndsAt == null || b.EndsAt > utcNow);

            if (isStillBanned)
            {
                throw new UnauthorizedAccessException("This account is currently banned.");
            }

            // Авто-разбан
            foreach (var block in activeBlocks)
            {
                block.Deactivate(utcNow);
            }

            user.SetAccountState(AccountState.Active, utcNow);
        }
    }
}
