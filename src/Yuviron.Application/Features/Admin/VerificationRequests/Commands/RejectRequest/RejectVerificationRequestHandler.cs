using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Commands.RejectRequest;

public sealed class RejectVerificationRequestHandler : IRequestHandler<RejectVerificationRequestCommand, Unit>
{
    private readonly IAuditingContext _auditingContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IEventBus _eventBus;

    public RejectVerificationRequestHandler(
        IAuditingContext auditingContext, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser,
        IEventBus eventBus) 
    {
        _auditingContext = auditingContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(RejectVerificationRequestCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var verificationReq = await _auditingContext.VerificationRequests
            .Include(vr => vr.Artist) 
            .FirstOrDefaultAsync(vr => vr.Id == request.RequestId, cancellationToken);

        if (verificationReq == null)
            throw new NotFoundException(nameof(VerificationRequest), request.RequestId);

        if (verificationReq.Status != VerificationRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        verificationReq.Reject(adminId, request.AdminNote, utcNow);

        await _auditingContext.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new ArtistClaimRejectedEvent(
            verificationReq.SubmittedByUserId,
            verificationReq.ArtistId,
            verificationReq.Artist.Name,
            request.AdminNote), cancellationToken);

        return Unit.Value;
    }
}