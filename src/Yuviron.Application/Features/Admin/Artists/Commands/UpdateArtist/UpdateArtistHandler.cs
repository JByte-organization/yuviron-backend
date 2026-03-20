using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events; // <-- ДОБАВИЛИ ДЛЯ ИВЕНТОВ
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistHandler : IRequestHandler<UpdateArtistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    // Убрали IFileStorageService
    public UpdateArtistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider) 
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateArtistCommand request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
                         .Include(a => a.TeamMembers) 
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var ownerExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.OwnerUserId, cancellationToken);

        if (!ownerExists) throw new NotFoundException(nameof(User), request.OwnerUserId);

        var oldAvatarUrl = artist.AvatarUrl;
        var oldBannerUrl = artist.BannerUrl;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var finalAvatarUrl = FileStorageExtensions.PredictDestinationPath(request.AvatarUrl, "avatars");
        var finalBannerUrl = FileStorageExtensions.PredictDestinationPath(request.BannerUrl, "uploads");
        
        artist.UpdateDetails(
            request.Name,
            request.Bio,
            finalAvatarUrl,
            finalBannerUrl,
            request.VerificationStatus,
            utcNow);

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl) && request.AvatarUrl.StartsWith("temp/"))
            artist.AddDomainEvent(new TempFileNeedsMovingEvent(request.AvatarUrl, "avatars"));

        if (!string.IsNullOrWhiteSpace(request.BannerUrl) && request.BannerUrl.StartsWith("temp/"))
            artist.AddDomainEvent(new TempFileNeedsMovingEvent(request.BannerUrl, "uploads"));

        if (!string.Equals(oldAvatarUrl, finalAvatarUrl, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(oldAvatarUrl))
            artist.AddDomainEvent(new FileNeedsDeletionEvent(oldAvatarUrl));

        if (!string.Equals(oldBannerUrl, finalBannerUrl, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(oldBannerUrl))
            artist.AddDomainEvent(new FileNeedsDeletionEvent(oldBannerUrl));

        var currentOwner = artist.TeamMembers.FirstOrDefault(tm => tm.Role == ArtistTeamRole.Owner);
        var newOwnerId = request.OwnerUserId;

        if (currentOwner?.UserId != newOwnerId)
        {
            if (currentOwner != null)
            {
                artist.UpdateTeamMemberRole(currentOwner.UserId, ArtistTeamRole.Manager, utcNow);
            }

            var isNewOwnerInTeam = artist.TeamMembers.Any(tm => tm.UserId == newOwnerId);

            if (isNewOwnerInTeam)
            {
                artist.UpdateTeamMemberRole(newOwnerId, ArtistTeamRole.Owner, utcNow);
            }
            else
            {
                artist.AddTeamMember(newOwnerId, ArtistTeamRole.Owner, utcNow);
            }

            var newOwnerUser = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == newOwnerId, cancellationToken);
                
            if (newOwnerUser != null) 
            {
                var managementRoleStr = nameof(RoleName.ManagementUser);
                var managementRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == managementRoleStr, cancellationToken)
                    ?? throw new InvalidOperationException($"Role '{managementRoleStr}' not found.");

                var currentRoleIds = newOwnerUser.UserRoles.Select(ur => ur.RoleId).ToList();
                if (!currentRoleIds.Contains(managementRole.Id))
                {
                    currentRoleIds.Add(managementRole.Id);
                    newOwnerUser.SyncRoles(currentRoleIds);
                }

                newOwnerUser.AddDomainEvent(new UserPermissionsChangedEvent(newOwnerId));
            }

            if (currentOwner != null)
            {
                var oldOwnerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentOwner.UserId, cancellationToken);
                if (oldOwnerUser != null) oldOwnerUser.AddDomainEvent(new UserPermissionsChangedEvent(oldOwnerUser.Id));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}