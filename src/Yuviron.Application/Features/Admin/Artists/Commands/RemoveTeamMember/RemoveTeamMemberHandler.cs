using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.RemoveTeamMember;

public sealed class RemoveTeamMemberHandler : IRequestHandler<RemoveTeamMemberCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<RemoveTeamMemberHandler> _logger;

    public RemoveTeamMemberHandler(
        IIdentityContext identityContext, ICatalogContext catalogContext, 
        TimeProvider timeProvider,
        IPermissionService permissionService,
        ILogger<RemoveTeamMemberHandler> logger)
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var artist = await _catalogContext.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        bool wasRemoved = artist.RemoveTeamMember(request.UserId, utcNow);

        if (!wasRemoved)
        {
            return Unit.Value; 
        }

        var belongsToOtherArtists = await _catalogContext.ArtistTeamMembers
            .AnyAsync(tm => tm.UserId == request.UserId && tm.ArtistId != request.ArtistId, cancellationToken);

        var user = await _identityContext.Users
            .Include(u => u.UserRoles) 
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user != null)
        {
            if (!belongsToOtherArtists)
            {
                var managementRoleStr = nameof(RoleName.ManagementUser);
                var managementRoleId = await _identityContext.Roles
                    .Where(r => r.Name == managementRoleStr)
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (managementRoleId != Guid.Empty)
                {
                    var currentRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
                    
                    if (currentRoleIds.Contains(managementRoleId))
                    {
                        currentRoleIds.Remove(managementRoleId);
                        
                        user.SyncRoles(currentRoleIds);
                    }
                }
            }

            user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id));
        }

        await _identityContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _permissionService.InvalidatePermissionsAsync(request.UserId, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache synchronously for User {UserId}. Outbox worker will retry.", request.UserId);
        }

        return Unit.Value;
    }
}