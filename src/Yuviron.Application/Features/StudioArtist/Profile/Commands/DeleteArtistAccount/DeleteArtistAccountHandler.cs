using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.DeleteArtistAccount;

public sealed class DeleteArtistAccountHandler : IRequestHandler<DeleteArtistAccountCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<DeleteArtistAccountHandler> _logger;

    public DeleteArtistAccountHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser,
        IPermissionService permissionService,
        ILogger<DeleteArtistAccountHandler> _logger)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
        _permissionService = permissionService;
        this._logger = _logger;
    }

    public async Task<Unit> Handle(DeleteArtistAccountCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        // CHECK PERMISSION: Only Owner can delete artist account
        var hasPermission = await _context.ArtistTeamMembers
            .HasFullAccess(request.ArtistId, currentUserId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("Only the Artist Owner can delete the account.");

        var artist = await _context.Artists
            .Include(a => a.TeamMembers)
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        artist.Delete(utcNow);

        // REVOKE ROLES logic (same as in Admin handler)
        var memberUserIds = artist.TeamMembers.Select(tm => tm.UserId).ToList();
        var usersWithOtherArtists = await _context.ArtistTeamMembers
            .Where(tm => memberUserIds.Contains(tm.UserId) && tm.ArtistId != request.ArtistId) 
            .Select(tm => tm.UserId).Distinct().ToListAsync(cancellationToken);

        var userIdsToRevokeRole = memberUserIds.Except(usersWithOtherArtists).ToList();

        if (userIdsToRevokeRole.Any())
        {
            var managementRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == nameof(RoleName.ManagementUser), cancellationToken);
            if (managementRole != null)
            {
                var users = await _context.Users.Include(u => u.UserRoles).Where(u => userIdsToRevokeRole.Contains(u.Id)).ToListAsync(cancellationToken);
                foreach (var user in users)
                {
                    user.SyncRoles(user.UserRoles.Where(ur => ur.RoleId != managementRole.Id).Select(ur => ur.RoleId));
                    user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id));
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        foreach (var userId in userIdsToRevokeRole)
        {
            try
            {
                await _permissionService.InvalidatePermissionsAsync(userId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache for User {UserId}.", userId);
            }
        }
        
        return Unit.Value;
    }
}
