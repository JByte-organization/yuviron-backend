using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities; 

namespace Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistHandler : IRequestHandler<CreatePlaylistCommand, Guid>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public CreatePlaylistHandler(
        ILibraryContext libraryContext, ISystemContext systemContext, 
        TimeProvider timeProvider, 
        ICurrentUserService currentUser)
    {
        _libraryContext = libraryContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        ClaimedFileResult? coverClaim = null;

        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _systemContext.ClaimFileAsync(
                request.CoverFileId.Value, userId, "image/", "covers", cancellationToken);
        }
        
        var playlist = Yuviron.Domain.Entities.Playlist.Create(
            userId: userId,
            artistId: null, 
            title: request.Title, 
            description: null, 
            coverUrl: coverClaim?.FinalPath,
            visibility: request.Visibility,
            isEditorial: false, 
            utcNow: utcNow
        );
        
        if (coverClaim != null)
        {
            playlist.RegisterFileSwapEvents(coverClaim);
        }

        _libraryContext.Add(playlist);
        await _libraryContext.SaveChangesAsync(cancellationToken);

        return playlist.Id;
    }
}