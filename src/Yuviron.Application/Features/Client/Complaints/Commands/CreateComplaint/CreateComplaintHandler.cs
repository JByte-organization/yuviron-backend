using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Complaints.Commands.CreateComplaint;

public sealed class CreateComplaintHandler : IRequestHandler<CreateComplaintCommand, Guid>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly IAuditingContext _auditingContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public CreateComplaintHandler(
        IIdentityContext identityContext, ICatalogContext catalogContext, ILibraryContext libraryContext, IAuditingContext auditingContext,
        ICurrentUserService currentUser,
        TimeProvider timeProvider)
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _auditingContext = auditingContext;
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

        _auditingContext.Add(complaint);

        var counter = await _auditingContext.ComplaintCounters
            .FirstOrDefaultAsync(x => x.TargetType == request.TargetType && x.TargetId == request.TargetId, cancellationToken);

        if (counter == null)
        {
            _auditingContext.Add(ComplaintCounter.Create(request.TargetType, request.TargetId, utcNow));
        }
        else
        {
            counter.IncrementNew(utcNow);
        }

        await _identityContext.SaveChangesAsync(cancellationToken);
        return complaint.Id;
    }

    private async Task EnsureTargetExistsAsync(ComplaintTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        var exists = targetType switch
        {
            ComplaintTargetType.Track => await _catalogContext.Tracks.AnyAsync(x => x.Id == targetId, cancellationToken),
            ComplaintTargetType.Album => await _catalogContext.Albums.AnyAsync(x => x.Id == targetId, cancellationToken),
            ComplaintTargetType.Artist => await _catalogContext.Artists.AnyAsync(x => x.Id == targetId, cancellationToken),
            ComplaintTargetType.User => await _identityContext.Users.AnyAsync(x => x.Id == targetId, cancellationToken),
            ComplaintTargetType.Playlist => await _libraryContext.Playlists.AnyAsync(x => x.Id == targetId, cancellationToken),
            _ => false
        };

        if (!exists)
        {
            throw new InvalidOperationException("Complaint target was not found.");
        }
    }
}
