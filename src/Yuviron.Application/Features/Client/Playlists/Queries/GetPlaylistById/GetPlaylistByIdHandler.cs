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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPlaylistByIdHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PlaylistDetailsClientDto> Handle(GetPlaylistByIdQuery request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
            .AsNoTracking()
            .Include(p => p.User).ThenInclude(u => u.Profile)
            .Include(p => p.PlaylistTracks).ThenInclude(pt => pt.Track) // Инклудим треки, чтобы посчитать их длительность
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (playlist == null)
            throw new NotFoundException("Playlist", request.Id);

        
        if (playlist.Visibility == PlaylistVisibility.Private && playlist.UserId != _currentUser.UserId)
        {
            throw new ForbiddenException("This playlist is private.");
        }

        int totalDurationMs = playlist.PlaylistTracks.Sum(pt => pt.Track?.DurationMs ?? 0);
        
        string creatorName = playlist.IsEditorial 
            ? "Yuviron" 
            : (playlist.User?.Profile?.FirstName ?? "Unknown User");

        return new PlaylistDetailsClientDto(
            playlist.Id,
            playlist.Title,
            playlist.Description,
            playlist.CoverUrl,
            playlist.Visibility,
            playlist.IsEditorial ? null : playlist.UserId,
            creatorName,
            playlist.IsEditorial,
            playlist.PlaylistTracks.Count,
            totalDurationMs,
            playlist.CreatedAt,
            playlist.UpdatedAt
        );
    }
}