using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistById;

public sealed class GetPlaylistByIdHandler : IRequestHandler<GetPlaylistByIdQuery, PlaylistDetailsClientDto>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;

    public GetPlaylistByIdHandler(ILibraryContext libraryContext, ICurrentUserService currentUser)
    {
        _libraryContext = libraryContext;
        _currentUser = currentUser;
    }

    public async Task<PlaylistDetailsClientDto> Handle(GetPlaylistByIdQuery request, CancellationToken cancellationToken)
    {
        var playlist = await _libraryContext.Playlists
            .AsNoTracking()
            .Include(p => p.User).ThenInclude(u => u!.Profile)
            .Include(p => p.Artist) 
            .Include(p => p.PlaylistTracks).ThenInclude(pt => pt.Track) 
            .FirstOrDefaultAsync(p => p.Id == request.Id , cancellationToken);

        if (playlist == null)
            throw new NotFoundException("Playlist", request.Id);

        if (playlist.Visibility == PlaylistVisibility.Private && playlist.UserId != _currentUser.UserId)
        {
            throw new ForbiddenException("This playlist is private.");
        }

        int totalDurationMs = playlist.PlaylistTracks
            .Where(pt => pt.Track != null && !pt.Track.IsDeleted)
            .Sum(pt => pt.Track!.DurationMs);
        
        string creatorName;
        if (playlist.IsEditorial) creatorName = "Yuviron";
        else if (playlist.ArtistId.HasValue && playlist.Artist != null) creatorName = playlist.Artist.Name;
        else creatorName = playlist.User?.Profile?.FirstName ?? "Unknown User";

        return new PlaylistDetailsClientDto(
            playlist.Id,
            playlist.Title,
            playlist.Description,
            playlist.CoverUrl,
            playlist.Visibility,
            playlist.IsEditorial ? null : (playlist.ArtistId ?? playlist.UserId), 
            creatorName,
            playlist.IsEditorial,
            playlist.PlaylistTracks.Count(pt => pt.Track != null && !pt.Track.IsDeleted),
            totalDurationMs,
            playlist.CreatedAt,
            playlist.UpdatedAt
        );
    }
}