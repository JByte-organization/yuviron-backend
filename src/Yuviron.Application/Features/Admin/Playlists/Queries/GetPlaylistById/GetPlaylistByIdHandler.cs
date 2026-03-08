using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                               p.IsPublic,
                               p.IsEditorial,
                               p.PlaylistTracks
                                   .OrderBy(pt => pt.Position)
                                   .Select(pt => new PlaylistTrackDto(
                                       pt.TrackId,
                                       pt.Track.Title,
                                       pt.Track.TrackArtists.FirstOrDefault() != null 
                                           ? pt.Track.TrackArtists.FirstOrDefault()!.Artist.Name 
                                           : null,
                                       pt.Position
                                   )).ToList()
                           ))
                           .FirstOrDefaultAsync(cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.Id);

        return playlist;
    }
}