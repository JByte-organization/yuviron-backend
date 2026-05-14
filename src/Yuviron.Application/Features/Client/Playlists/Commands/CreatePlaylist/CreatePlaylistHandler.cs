using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistHandler : IRequestHandler<CreatePlaylistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public CreatePlaylistHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");
        
        var playlist = Domain.Entities.Playlist.Create(
            userId: userId,
            title: request.Name,
            description: null, 
            finalCoverUrl,
            visibility: request.Visibility,
            isEditorial: false,
            utcNow: utcNow
        );
        
        if (!string.IsNullOrWhiteSpace(request.CoverUrl) && request.CoverUrl.StartsWith("temp/"))
        {
            playlist.AddDomainEvent(new TempFileNeedsMovingEvent(request.CoverUrl, "covers"));
        }

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync(cancellationToken);

        return playlist.Id;
    }
}