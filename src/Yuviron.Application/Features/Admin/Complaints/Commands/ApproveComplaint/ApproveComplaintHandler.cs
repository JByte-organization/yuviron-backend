using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Complaints.Commands.ApproveComplaint;

public sealed class ApproveComplaintHandler : IRequestHandler<ApproveComplaintCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IProfileContext _profileContext;
    private readonly IAuditingContext _auditingContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public ApproveComplaintHandler(
        ICatalogContext catalogContext, IProfileContext profileContext, IAuditingContext auditingContext,
        ICurrentUserService currentUser,
        TimeProvider timeProvider,
        IEventBus eventBus)
    {
        _catalogContext = catalogContext;
        _profileContext = profileContext;
        _auditingContext = auditingContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(ApproveComplaintCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var complaint = await _auditingContext.Complaints
            .FirstOrDefaultAsync(x => x.Id == request.ComplaintId, cancellationToken)
            ?? throw new NotFoundException(nameof(Complaint), request.ComplaintId);

        if (complaint.Status != ComplaintStatus.New)
        {
            throw new InvalidOperationException("Only new complaints can be reviewed.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        complaint.MarkAsInReview(adminId, utcNow);
        complaint.Approve(adminId, request.AdminNote, utcNow);

        var counter = await _auditingContext.ComplaintCounters
            .FirstOrDefaultAsync(x => x.TargetType == complaint.TargetType && x.TargetId == complaint.TargetId, cancellationToken);

        counter?.ResolveOpen(utcNow);

        await _catalogContext.SaveChangesAsync(cancellationToken);

        var targetTitle = await ResolveTargetTitleAsync(complaint.TargetType, complaint.TargetId, cancellationToken);

        await _eventBus.PublishAsync(
            new ComplaintApprovedEvent(
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
            ComplaintTargetType.Track => await _catalogContext.Tracks.AsNoTracking().Where(x => x.Id == targetId).Select(x => x.Title).FirstOrDefaultAsync(ct) ?? targetType.ToString(),
            ComplaintTargetType.Album => await _catalogContext.Albums.AsNoTracking().Where(x => x.Id == targetId).Select(x => x.Title).FirstOrDefaultAsync(ct) ?? targetType.ToString(),
            ComplaintTargetType.Artist => await _catalogContext.Artists.AsNoTracking().Where(x => x.Id == targetId).Select(x => x.Name).FirstOrDefaultAsync(ct) ?? targetType.ToString(),
            ComplaintTargetType.User => await _profileContext.UserProfiles.AsNoTracking().Where(x => x.Id == targetId).Select(x => x.FirstName).FirstOrDefaultAsync(ct) ?? targetType.ToString(),
            _ => targetType.ToString()
        };
}
