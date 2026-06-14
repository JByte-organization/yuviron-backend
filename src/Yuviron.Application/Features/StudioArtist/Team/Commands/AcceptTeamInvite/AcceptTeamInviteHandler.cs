using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.StudioArtist.Team.Commands.AddTeamMember;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Team.Commands.AcceptTeamInvite;

public sealed class AcceptTeamInviteHandler : IRequestHandler<AcceptTeamInviteCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus; 

    public AcceptTeamInviteHandler(
        IIdentityContext identityContext, ICatalogContext catalogContext, 
        ICurrentUserService currentUser, 
        ICacheService cache,
        TimeProvider timeProvider,
        IEventBus eventBus) 
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext; _currentUser = currentUser; _cache = cache; _timeProvider = timeProvider; _eventBus = eventBus;
    }

    public async Task<Unit> Handle(AcceptTeamInviteCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var inviteData = await _cache.GetAsync<TeamInvitationData>($"team_invite:{request.Token}", cancellationToken)
            ?? throw new InvalidOperationException("Invitation link is invalid or has expired.");

        var currentUser = await _identityContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

        if (currentUser?.Email != inviteData.UserEmail)
        {
            throw new InvalidOperationException("This invitation was sent to a different email address.");
        }

        var artist = await _catalogContext.Artists
            .Include(a => a.TeamMembers)
            .FirstOrDefaultAsync(a => a.Id == inviteData.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), inviteData.ArtistId);

        artist.AddTeamMember(currentUserId, inviteData.Role, utcNow);

        await _identityContext.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync($"team_invite:{request.Token}", cancellationToken);

        await _eventBus.PublishAsync(new TeamMemberJoinedEvent(
            artist.Id,
            artist.Name,
            inviteData.UserEmail,
            inviteData.Role
        ), cancellationToken);

        return Unit.Value;
    }
}