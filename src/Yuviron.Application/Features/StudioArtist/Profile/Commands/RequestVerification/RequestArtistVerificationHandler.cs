using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.RequestVerification;

public sealed class RequestArtistVerificationHandler : IRequestHandler<RequestArtistVerificationCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IAuditingContext _auditingContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public RequestArtistVerificationHandler(ICatalogContext catalogContext, IAuditingContext auditingContext, ISystemContext systemContext, TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _auditingContext = auditingContext;
        _systemContext = systemContext; _timeProvider = timeProvider; _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RequestArtistVerificationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var artist = await _catalogContext.Artists
            .Include(a => a.TeamMembers)
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        if (!artist.TeamMembers.Any(tm => tm.UserId == userId && tm.Role == ArtistTeamRole.Owner))
            throw new ForbiddenException("Only the Owner of the artist profile can request verification.");

        var existingRequest = await _auditingContext.VerificationRequests
            .AnyAsync(vr => vr.ArtistId == request.ArtistId && vr.Status == VerificationRequestStatus.Pending, cancellationToken);

        if (existingRequest)
            throw new InvalidOperationException("A verification request is already pending for this artist.");

        ClaimedFileResult? proofFileClaim = null;
        if (request.ProofFileId.HasValue)
        {
            proofFileClaim = await _systemContext.ClaimFileAsync(
                request.ProofFileId.Value, userId, "image/", "proofs", cancellationToken);
        }

        var verificationReq = VerificationRequest.Create(
            artistId: request.ArtistId,
            userId: userId,
            claimedRole: ClaimRole.Artist, 
            officialEmail: request.OfficialEmail,
            links: request.Links,
            proofFileUrl: proofFileClaim?.FinalPath,
            message: request.Message,
            utcNow: utcNow
        );

        _auditingContext.Add(verificationReq);
        await _catalogContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}