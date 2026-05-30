using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Commands.RejectRequest;

public sealed class RejectVerificationRequestHandler : IRequestHandler<RejectVerificationRequestCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public RejectVerificationRequestHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RejectVerificationRequestCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var verificationReq = await _context.VerificationRequests
            .FirstOrDefaultAsync(vr => vr.Id == request.RequestId, cancellationToken);

        if (verificationReq == null)
            throw new NotFoundException(nameof(VerificationRequest), request.RequestId);

        if (verificationReq.Status != VerificationRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        verificationReq.Reject(adminId, request.AdminNote, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}