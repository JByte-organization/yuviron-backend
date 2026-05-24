using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.CreateProfile;

public sealed class CreateArtistProfileHandler : IRequestHandler<CreateArtistProfileCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ArtistLimitsOptions _limits;
    private readonly ICurrentUserService _currentUser;

    public CreateArtistProfileHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IOptions<ArtistLimitsOptions> options,
        ICurrentUserService currentUser) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _limits = options.Value; 
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateArtistProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var ownedArtistsCount = await _context.ArtistTeamMembers
            .AsNoTracking()
            .CountAsync(atm => atm.UserId == userId && atm.Role == ArtistTeamRole.Owner, cancellationToken);

        var isPremium = await _context.Users
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
            avatarClaim = await _context.ClaimFileAsync(
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

        _context.Artists.Add(artist);
        await _context.SaveChangesAsync(cancellationToken);

        return artist.Id;
    }
}