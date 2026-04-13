using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistById;

public sealed class GetPlaylistByIdHandler : IRequestHandler<GetPlaylistByIdQuery, PlaylistDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetPlaylistByIdHandler(IApplicationDbContext context) => _context = context;

    public async Task<PlaylistDetailsDto> Handle(GetPlaylistByIdQuery request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
                           .AsNoTracking()
                           .Where(p => p.Id == request.Id)
                           .Select(p => new PlaylistDetailsDto(
                               p.Id,
                               p.Title,
                               p.Description,
                               p.CoverUrl,
                               p.Visibility,
                               p.IsEditorial,
                           
                               p.IsEditorial || p.User == null ? null : new PlaylistCreatorDto(
                                   p.UserId,
                                   p.User.Profile.FirstName,
                                   p.User.Email,
                                   p.User.Profile.AvatarUrl 
                               ),
                           
                               p.PlaylistTracks.Count,
                               p.CreatedAt,
                               p.UpdatedAt
                           ))
                           .FirstOrDefaultAsync(cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.Id);

        return playlist;
    }
}