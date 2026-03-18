using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; // <-- Нужен для файлов
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistHandler : IRequestHandler<CreatePlaylistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService; // <-- Добавили

    public CreatePlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider, 
        ICurrentUserService currentUser,
        IFileStorageService fileStorageService) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
    }

    public async Task<Guid> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var userId = request.IsEditorial ? null : _currentUser.UserId;

        var finalCoverUrl = await _fileStorageService.MoveIfTempAsync(request.CoverUrl, "covers", cancellationToken);

        var playlist = Playlist.Create(
            userId,
            request.Title,
            request.Description,
            finalCoverUrl, 
            request.Visibility,
            request.IsEditorial,
            utcNow
        );

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync(cancellationToken);

        return playlist.Id;
    }
}