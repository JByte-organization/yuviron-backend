using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistHandler : IRequestHandler<UpdateArtistCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly IIdentityManager _identityManager;
    private readonly ICurrentUserService _currentUser;

    public UpdateArtistHandler(
        IIdentityContext identityContext, ICatalogContext catalogContext, ISystemContext systemContext, 
        TimeProvider timeProvider,
        IIdentityManager identityManager,
        ICurrentUserService currentUser) 
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _identityManager = identityManager;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateArtistCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var artist = await _catalogContext.Artists
            .Include(a => a.TeamMembers) 
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken);

        if (artist == null)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        if (request.OwnerUserId.HasValue)
        {
            var ownerExists = await _identityContext.Users.AnyAsync(u => u.Id == request.OwnerUserId.Value, cancellationToken);
            if (!ownerExists)
            {
                throw new NotFoundException(nameof(User), request.OwnerUserId.Value);
            }
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        string? finalAvatarUrl = artist.AvatarUrl;
        if (request.AvatarFileId.HasValue)
        {
            var avatarClaim = await _systemContext.ClaimFileAsync(
                request.AvatarFileId.Value, adminId, "image/", "avatars", cancellationToken);
            
            artist.RegisterFileSwapEvents(avatarClaim, artist.AvatarUrl);
            finalAvatarUrl = avatarClaim.FinalPath;
        }

        string? finalBannerUrl = artist.BannerUrl;
        if (request.BannerFileId.HasValue)
        {
            var bannerClaim = await _systemContext.ClaimFileAsync(
                request.BannerFileId.Value, adminId, "image/", "banners", cancellationToken);
            
            artist.RegisterFileSwapEvents(bannerClaim, artist.BannerUrl);
            finalBannerUrl = bannerClaim.FinalPath;
        }

        artist.UpdateDetails(
            request.Name,
            request.Bio,
            finalAvatarUrl,
            finalBannerUrl,
            request.VerificationStatus,
            utcNow);

        var currentOwner = artist.TeamMembers.FirstOrDefault(tm => tm.Role == ArtistTeamRole.Owner);
        var newOwnerId = request.OwnerUserId;

        if (currentOwner?.UserId != newOwnerId)
        {
            if (currentOwner != null)
            {
                artist.UpdateTeamMemberRole(currentOwner.UserId, ArtistTeamRole.Manager, utcNow);
            }

            if (newOwnerId.HasValue)
            {
                var isNewOwnerInTeam = artist.TeamMembers.Any(tm => tm.UserId == newOwnerId.Value);

                if (isNewOwnerInTeam)
                {
                    artist.UpdateTeamMemberRole(newOwnerId.Value, ArtistTeamRole.Owner, utcNow);
                }
                else
                {
                    artist.AddTeamMember(newOwnerId.Value, ArtistTeamRole.Owner, utcNow);
                }

                await _identityManager.EnsureManagementRoleAsync(newOwnerId.Value, cancellationToken);
            }
        }

        await _identityContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}