using Yuviron.Application.Abstractions.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication; // <-- ИСПРАВЛЕНО (добавили Authentication)
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.CreateProfile;

public sealed class CreateArtistProfileHandler : IRequestHandler<CreateArtistProfileCommand, Guid>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ArtistLimitsOptions _limits;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;

    public CreateArtistProfileHandler(
        IIdentityContext identityContext, ICatalogContext catalogContext, ISystemContext systemContext, 
        TimeProvider timeProvider,
        IOptions<ArtistLimitsOptions> options,
        ICurrentUserService currentUser,
        IPermissionService permissionService) 
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _limits = options.Value; 
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<Guid> Handle(CreateArtistProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var ownedArtistsCount = await _catalogContext.ArtistTeamMembers
            .AsNoTracking()
            .CountAsync(atm => atm.UserId == userId && atm.Role == ArtistTeamRole.Owner, cancellationToken);

        var isPremium = await _identityContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .AnyAsync(u => u.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow), cancellationToken);

        int maxProfiles = isPremium ? _limits.PremiumUserMaxProfiles : _limits.FreeUserMaxProfiles; 

        if (ownedArtistsCount >= maxProfiles)
        {
            throw new ForbiddenException($"Limit exceeded. Your current plan allows managing up to {maxProfiles} artist profile(s).");
        }

        ClaimedFileResult? avatarClaim = null;
    
        if (request.AvatarFileId.HasValue)
        {
            avatarClaim = await _systemContext.ClaimFileAsync(
                request.AvatarFileId.Value, userId, "image/", "avatars", cancellationToken);
        }

        var artist = Artist.Create(
            initialOwnerUserId: userId,
            name: request.Name,
            bio: null, 
            avatarUrl: avatarClaim?.FinalPath,
            bannerUrl: null,
            verificationStatus: VerificationStatus.None,
            utcNow: utcNow
        );

        if (avatarClaim != null)
        {
            artist.RegisterFileSwapEvents(avatarClaim);
        }

        _catalogContext.Add(artist);
        
        var user = await _identityContext.Users
            .Include(u => u.UserRoles)
            .FirstAsync(u => u.Id == userId, cancellationToken);
            
        var managementRole = await _identityContext.Roles
            .FirstAsync(r => r.Name == nameof(RoleName.ManagementUser), cancellationToken);

        bool roleAdded = false;
        
        if (!user.UserRoles.Any(ur => ur.RoleId == managementRole.Id))
        {
            user.UserRoles.Add(new UserRole(user.Id, managementRole.Id));
            roleAdded = true;
        }
        
        await _identityContext.SaveChangesAsync(cancellationToken);

        if (roleAdded)
        {
            await _permissionService.InvalidatePermissionsAsync(userId, cancellationToken);
        }

        return artist.Id;
    }
}