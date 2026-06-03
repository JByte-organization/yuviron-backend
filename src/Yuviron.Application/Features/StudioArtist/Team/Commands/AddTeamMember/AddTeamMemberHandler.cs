using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Domain.Entities;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Events; // Потрібно створити івент

namespace Yuviron.Application.Features.StudioArtist.Team.Commands.AddTeamMember;

// Дані, які ми покладемо в Redis під токеном
public record TeamInvitationData(Guid ArtistId, string UserEmail, ArtistTeamRole Role);

public sealed class AddTeamMemberHandler : IRequestHandler<AddTeamMemberCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus; // MassTransit

    public AddTeamMemberHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser, 
        ICacheService cache, 
        IEventBus eventBus)
    {
        _context = context; _currentUser = currentUser; _cache = cache; _eventBus = eventBus;
    }

    public async Task<Unit> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var isOwner = await _context.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == currentUserId && tm.Role == ArtistTeamRole.Owner, cancellationToken);
        
        if (!isOwner) throw new ForbiddenException("Only the Owner can invite team members.");

        var artist = await _context.Artists
                         .AsNoTracking()
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        // Генеруємо унікальний токен для листа
        var inviteToken = Guid.NewGuid().ToString("N");
        
        var inviteData = new TeamInvitationData(request.ArtistId, request.UserEmail, request.Role);
        
        // Зберігаємо в Redis на 48 годин
        await _cache.SetAsync($"team_invite:{inviteToken}", inviteData, TimeSpan.FromHours(48), cancellationToken);

        await _eventBus.PublishAsync(new SendTeamInviteEmailEvent(
            request.UserEmail,
            artist.Name,
            request.Role.ToString(),
            inviteToken
        ), cancellationToken);

        return Unit.Value;
    }
}