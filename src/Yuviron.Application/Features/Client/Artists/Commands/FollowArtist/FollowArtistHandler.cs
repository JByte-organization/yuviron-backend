using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging; // Подключаем шину
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events; // Подключаем ивент
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Commands.FollowArtist;

public sealed class FollowArtistHandler : IRequestHandler<FollowArtistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventBus _eventBus; // Добавили шину
    private readonly TimeProvider _timeProvider;

    public FollowArtistHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService, 
        IEventBus eventBus, 
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _eventBus = eventBus;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(FollowArtistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var artistExists = await _context.Artists.AnyAsync(a => a.Id == request.ArtistId, cancellationToken);
        if (!artistExists) throw new NotFoundException(nameof(Artist), request.ArtistId);

        var alreadyFollowing = await _context.UserFollowArtists
            .AnyAsync(f => f.UserId == userId && f.ArtistId == request.ArtistId, cancellationToken);

        if (alreadyFollowing) return Unit.Value; 

        _context.UserFollowArtists.Add(new UserFollowArtist(
            userId, 
            request.ArtistId, 
            true,
            _timeProvider.GetUtcNow().UtcDateTime
        ));
        
        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new UserFollowedArtistEvent(userId, request.ArtistId), cancellationToken);

        return Unit.Value;
    }
}