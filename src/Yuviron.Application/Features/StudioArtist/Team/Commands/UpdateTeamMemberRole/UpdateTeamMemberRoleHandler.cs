using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Messaging; // <-- ДОДАНО
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events; // <-- ДОДАНО
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Team.Commands.UpdateTeamMemberRole;

public sealed class UpdateTeamMemberRoleHandler : IRequestHandler<UpdateTeamMemberRoleCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus; // <-- ДОДАНО

    public UpdateTeamMemberRoleHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser, 
        TimeProvider timeProvider,
        IEventBus eventBus) // <-- ДОДАНО
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider; _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UpdateTeamMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var isOwner = await _context.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == currentUserId && tm.Role == ArtistTeamRole.Owner, cancellationToken);
        
        if (!isOwner) throw new ForbiddenException("Only the Owner can change team roles.");

        var artist = await _context.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        artist.UpdateTeamMemberRole(request.TargetUserId, request.NewRole, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new TeamRoleChangedEvent(
            request.TargetUserId,
            artist.Id,
            artist.Name,
            request.NewRole
        ), cancellationToken);

        return Unit.Value;
    }
}