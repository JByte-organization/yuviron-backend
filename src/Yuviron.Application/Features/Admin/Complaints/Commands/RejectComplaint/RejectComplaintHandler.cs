using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Complaints.Commands.RejectComplaint;

public sealed class RejectComplaintHandler : IRequestHandler<RejectComplaintCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public RejectComplaintHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        TimeProvider timeProvider,
        IEventBus eventBus)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(RejectComplaintCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(x => x.Id == request.ComplaintId, cancellationToken)
            ?? throw new NotFoundException(nameof(Complaint), request.ComplaintId);

        if (complaint.Status != ComplaintStatus.New)
        {
            throw new InvalidOperationException("Only new complaints can be reviewed.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        complaint.MarkAsInReview(adminId, utcNow);
        complaint.Reject(adminId, request.AdminNote, utcNow);

        var counter = await _context.ComplaintCounters
            .FirstOrDefaultAsync(x => x.TargetType == complaint.TargetType && x.TargetId == complaint.TargetId, cancellationToken);

        counter?.ResolveOpen(utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        var targetTitle = await ResolveTargetTitleAsync(complaint.TargetType, complaint.TargetId, cancellationToken);

        await _eventBus.PublishAsync(
            new ComplaintRejectedEvent(
                complaint.Id,
                complaint.CreatedByUserId,
                complaint.TargetType,
                complaint.TargetId,
                targetTitle,
                request.AdminNote),
            cancellationToken);

        return Unit.Value;
    }

    private async Task<string> ResolveTargetTitleAsync(ComplaintTargetType targetType, Guid targetId, CancellationToken ct) =>
        targetType switch
        {
            ComplaintTargetType.Track => await _context.Tracks.AsNoTracking().Where(x => x.Id == targetId).Select(x => x.Title).FirstOrDefaultAsync(ct) ?? targetType.ToString(),
            ComplaintTargetType.Album => await _context.Albums.AsNoTracking().Where(x => x.Id == targetId).Select(x => x.Title).FirstOrDefaultAsync(ct) ?? targetType.ToString(),
            ComplaintTargetType.Artist => await _context.Artists.AsNoTracking().Where(x => x.Id == targetId).Select(x => x.Name).FirstOrDefaultAsync(ct) ?? targetType.ToString(),
            ComplaintTargetType.User => await _context.UserProfiles.AsNoTracking().Where(x => x.Id == targetId).Select(x => x.FirstName).FirstOrDefaultAsync(ct) ?? targetType.ToString(),
            _ => targetType.ToString()
        };
}
