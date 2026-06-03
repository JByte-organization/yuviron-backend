using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.PublishAlbum;

public sealed class PublishAlbumHandler : IRequestHandler<PublishAlbumCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IEventBus _eventBus;
    private readonly TimeProvider _timeProvider;

    public PublishAlbumHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser, 
        IEventBus eventBus,
        TimeProvider timeProvider)
    {
        _context = context; _currentUser = currentUser; _eventBus = eventBus; _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(PublishAlbumCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var album = await _context.Albums
            .Include(a => a.AlbumArtists)
                .ThenInclude(aa => aa.Artist) 
            .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken)
            ?? throw new NotFoundException(nameof(Album), request.AlbumId);

        if (album.VisibilityStatus == VisibilityStatus.Published)
            throw new InvalidOperationException("Album is already published.");

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(album.AlbumArtists.Select(aa => aa.ArtistId), userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to publish this album.");

        album.UpdateDetails(album.Title, album.Description, album.CoverUrl, album.ReleaseDate, album.ReleaseType, 
            VisibilityStatus.Published, album.ScheduledPublishAt, album.AlbumArtists.Select(aa => aa.ArtistId).ToList(), utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        var mainArtist = album.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.Artist 
                         ?? album.AlbumArtists.First().Artist;

        await _eventBus.PublishAsync(new NewReleasePublishedEvent(
            mainArtist.Id,
            mainArtist.Name,
            album.Id,
            NotificationEntityType.Album,
            album.Title,
            album.CoverUrl
        ), cancellationToken);

        return Unit.Value;
    }
}