using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistHandler : IRequestHandler<CreatePlaylistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreatePlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider) 
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var userId = request.IsEditorial ? (Guid?)null : request.OwnerUserId;

        if (userId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId.Value, cancellationToken);
            if (!userExists) throw new NotFoundException(nameof(User), userId.Value);
        }

        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");

        var playlist = Playlist.Create(
            userId, 
            request.Title,
            request.Description,
            finalCoverUrl, 
            request.Visibility,
            request.IsEditorial,
            utcNow
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