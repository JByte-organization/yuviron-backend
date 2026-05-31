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

namespace Yuviron.Application.Features.Admin.VerificationRequests.Commands.ApproveRequest;

public sealed class ApproveVerificationRequestHandler : IRequestHandler<ApproveVerificationRequestCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;
    private readonly IEventBus _eventBus;

    public ApproveVerificationRequestHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser,
        IPermissionService permissionService, 
        IEventBus eventBus)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
        _permissionService = permissionService;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(ApproveVerificationRequestCommand command, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var request = await _context.VerificationRequests
            .Include(vr => vr.Artist)
                .ThenInclude(a => a.TeamMembers)
            .Include(vr => vr.SubmittedByUser)
                .ThenInclude(u => u.UserRoles)
            .FirstOrDefaultAsync(vr => vr.Id == command.RequestId, cancellationToken);

        if (request == null)
            throw new NotFoundException(nameof(VerificationRequest), command.RequestId);

        if (request.Status != VerificationRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved.");

        if (request.Artist.TeamMembers.Any(tm => tm.Role == ArtistTeamRole.Owner))
            throw new InvalidOperationException("This artist profile already has an owner.");

        request.Approve(adminId, command.AdminNote, utcNow);

        request.Artist.AddTeamMember(request.SubmittedByUserId, ArtistTeamRole.Owner, utcNow);

        request.Artist.UpdateDetails(
            request.Artist.Name, 
            request.Artist.Bio, 
            request.Artist.AvatarUrl, 
            request.Artist.BannerUrl, 
            VerificationStatus.Verified, 
            utcNow);

        var managementRole = await _context.Roles
            .FirstAsync(r => r.Name == nameof(RoleName.ManagementUser), cancellationToken);

        bool roleAdded = false;
        if (!request.SubmittedByUser.UserRoles.Any(ur => ur.RoleId == managementRole.Id))
        {
            request.SubmittedByUser.UserRoles.Add(new UserRole(request.SubmittedByUserId, managementRole.Id));
            roleAdded = true;
        }

        var otherPendingRequests = await _context.VerificationRequests
            .Where(vr => vr.ArtistId == request.ArtistId 
                         && vr.Id != request.Id 
                         && vr.Status == VerificationRequestStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var otherReq in otherPendingRequests)
        {
            otherReq.Reject(adminId, "Profile has been verified by another user.", utcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (roleAdded)
        {
            await _permissionService.InvalidatePermissionsAsync(request.SubmittedByUserId, cancellationToken);
        }
        
        await _eventBus.PublishAsync(new ArtistClaimApprovedEvent(
            request.SubmittedByUserId, 
            request.ArtistId, 
            request.Artist.Name), cancellationToken);
        
        return Unit.Value;
    }
}