using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistHandler : IRequestHandler<UpdatePlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService;

    public UpdatePlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider, 
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.Id);

        var oldCoverUrl = playlist.CoverUrl; 
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        playlist.Update(
            request.Title,
            request.Description,
            request.CoverUrl,
            request.Visibility,
            utcNow
        );

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.Equals(oldCoverUrl, request.CoverUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldCoverUrl))
        {
            await _fileStorageService.DeleteAsync(oldCoverUrl, cancellationToken);
        }

        return Unit.Value;
    }
}