using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Albums.Commands.DeleteAlbum;

public sealed class DeleteAlbumHandler : IRequestHandler<DeleteAlbumCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider; 
    private readonly IFileStorageService _fileStorageService; 

    public DeleteAlbumHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(DeleteAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await _context.Albums
                        .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Album), request.AlbumId);

        var coverUrlToDelete = album.CoverUrl;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        album.Delete(utcNow);
        
        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(coverUrlToDelete))
        {
            await _fileStorageService.DeleteAsync(coverUrlToDelete, cancellationToken);
        }

        return Unit.Value;
    }
}