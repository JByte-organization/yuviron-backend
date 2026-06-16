using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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

namespace Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.ClaimProfile;

public sealed class ClaimArtistProfileHandler : IRequestHandler<ClaimArtistProfileCommand>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly IAuditingContext _auditingContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ArtistLimitsOptions _limits;
    private readonly ICurrentUserService _currentUser;

    public ClaimArtistProfileHandler(
        IIdentityContext identityContext, ICatalogContext catalogContext, IAuditingContext auditingContext, ISystemContext systemContext, 
        TimeProvider timeProvider,
        IOptions<ArtistLimitsOptions> options,
        ICurrentUserService currentUser) 
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _auditingContext = auditingContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _limits = options.Value; 
        _currentUser = currentUser;
    }

    public async Task Handle(ClaimArtistProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var artist = await _catalogContext.Artists
            .Include(a => a.TeamMembers)
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken);

        if (artist == null)
            throw new NotFoundException(nameof(Artist), request.ArtistId);

        if (artist.TeamMembers.Any(tm => tm.Role == ArtistTeamRole.Owner))
            throw new InvalidOperationException("This artist profile has already been verified and claimed by someone else.");

        var existingRequest = await _auditingContext.VerificationRequests
            .AnyAsync(vr => vr.ArtistId == request.ArtistId 
                            && vr.SubmittedByUserId == userId 
                            && vr.Status == VerificationRequestStatus.Pending, 
                cancellationToken);

        if (existingRequest)
            throw new InvalidOperationException("You already have a pending verification request for this artist.");

        var ownedArtistsCount = await _catalogContext.ArtistTeamMembers
            .AsNoTracking()
            .CountAsync(atm => atm.UserId == userId && atm.Role == ArtistTeamRole.Owner, cancellationToken);

        var isPremium = await _identityContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow), cancellationToken);

        int maxProfiles = isPremium ? _limits.PremiumUserMaxProfiles : _limits.FreeUserMaxProfiles; 

        if (ownedArtistsCount >= maxProfiles)
            throw new ForbiddenException($"Limit exceeded. Your current plan allows claiming up to {maxProfiles} artist profile(s).");

        ClaimedFileResult? proofFileClaim = null;
        if (request.ProofFileId.HasValue)
        {
            proofFileClaim = await _systemContext.ClaimFileAsync(
                request.ProofFileId.Value, userId, "image/", "proofs", cancellationToken);
        }

        var verificationReq = VerificationRequest.Create(
            artistId: request.ArtistId,
            userId: userId,
            claimedRole: request.ClaimedRole,
            officialEmail: request.OfficialEmail,
            links: request.Links,
            proofFileUrl: proofFileClaim?.FinalPath,
            message: request.Message,
            utcNow: utcNow
        );

        _auditingContext.Add(verificationReq);

        if (proofFileClaim != null)
        {
            verificationReq.RegisterFileSwapEvents(proofFileClaim);
        }
        
        await _identityContext.SaveChangesAsync(cancellationToken);
    }
}