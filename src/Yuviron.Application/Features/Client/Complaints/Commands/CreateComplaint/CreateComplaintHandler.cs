using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Complaints.Commands.CreateComplaint;

public sealed class CreateComplaintHandler : IRequestHandler<CreateComplaintCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public CreateComplaintHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await EnsureTargetExistsAsync(request.TargetType, request.TargetId, cancellationToken);

        var complaint = Complaint.Create(
            userId,
            request.TargetType,
            request.TargetId,
            request.ReasonCode,
            request.Comment,
            utcNow);

        _context.Complaints.Add(complaint);

        var counter = await _context.ComplaintCounters
            .FirstOrDefaultAsync(x => x.TargetType == request.TargetType && x.TargetId == request.TargetId, cancellationToken);

        if (counter == null)
        {
            _context.ComplaintCounters.Add(ComplaintCounter.Create(request.TargetType, request.TargetId, utcNow));
        }
        else
        {
            counter.IncrementNew(utcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return complaint.Id;
    }

    private async Task EnsureTargetExistsAsync(ComplaintTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        var exists = targetType switch
        {
            ComplaintTargetType.Track => await _context.Tracks.AnyAsync(x => x.Id == targetId, cancellationToken),
            ComplaintTargetType.Album => await _context.Albums.AnyAsync(x => x.Id == targetId, cancellationToken),
            ComplaintTargetType.Artist => await _context.Artists.AnyAsync(x => x.Id == targetId, cancellationToken),
            ComplaintTargetType.User => await _context.Users.AnyAsync(x => x.Id == targetId, cancellationToken),
            _ => false
        };

        if (!exists)
        {
            throw new InvalidOperationException("Complaint target was not found.");
        }
    }
}
