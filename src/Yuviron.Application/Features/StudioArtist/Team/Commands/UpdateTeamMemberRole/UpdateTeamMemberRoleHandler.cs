using Yuviron.Application.Abstractions.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Team.Commands.UpdateTeamMemberRole;

public sealed class UpdateTeamMemberRoleHandler : IRequestHandler<UpdateTeamMemberRoleCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public UpdateTeamMemberRoleHandler(
        ICatalogContext catalogContext, 
        ICurrentUserService currentUser, 
        TimeProvider timeProvider,
        IEventBus eventBus)
    {
        _catalogContext = catalogContext; _currentUser = currentUser; _timeProvider = timeProvider; _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UpdateTeamMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var isOwner = await _catalogContext.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == currentUserId && tm.Role == ArtistTeamRole.Owner, cancellationToken);
        
        if (!isOwner) throw new ForbiddenException("Only the Owner can change team roles.");

        if (request.TargetUserId == currentUserId && request.NewRole != ArtistTeamRole.Owner)
        {
            throw new InvalidOperationException("Cannot demote the only owner. Transfer ownership to another member first.");
        }

        var artist = await _catalogContext.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        // To avoid MySQL unique constraint violation (two owners at once during update),
        // we can perform the demotion of the current owner first and save, then promote the new one.
        if (request.NewRole == ArtistTeamRole.Owner)
        {
            var currentOwner = artist.TeamMembers.FirstOrDefault(tm => tm.Role == ArtistTeamRole.Owner);
            if (currentOwner != null && currentOwner.UserId != request.TargetUserId)
            {
                // Demote existing owner to Manager first
                artist.UpdateTeamMemberRole(currentOwner.UserId, ArtistTeamRole.Manager, utcNow);
                await _catalogContext.SaveChangesAsync(cancellationToken);
            }
        }

        artist.UpdateTeamMemberRole(request.TargetUserId, request.NewRole, utcNow);
        await _catalogContext.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new TeamRoleChangedEvent(
            request.TargetUserId,
            artist.Id,
            artist.Name,
            request.NewRole
        ), cancellationToken);

        return Unit.Value;
    }
}
