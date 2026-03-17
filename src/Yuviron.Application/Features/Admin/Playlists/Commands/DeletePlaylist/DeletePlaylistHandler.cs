using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.DeletePlaylist;

public sealed class DeletePlaylistHandler : IRequestHandler<DeletePlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService;

    public DeletePlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(DeletePlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.Id);

        var coverUrlToDelete = playlist.CoverUrl; 
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        playlist.Delete(utcNow);

        await _context.SaveChangesAsync(cancellationToken);


        if (!string.IsNullOrWhiteSpace(coverUrlToDelete))
        {
            await _fileStorageService.DeleteAsync(coverUrlToDelete, cancellationToken);
        }

        return Unit.Value;
    }
}