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
                           .Include(p => p.PlaylistTracks)
                           .ThenInclude(pt => pt.Track)
                           .ThenInclude(t => t.TrackArtists)
                           .ThenInclude(ta => ta.Artist)
                           .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.Id);

        return new PlaylistDetailsDto(
            playlist.Id,
            playlist.Title,
            playlist.Description,
            playlist.CoverUrl,
            playlist.IsPublic,
            playlist.IsEditorial,
            playlist.PlaylistTracks
                .OrderBy(pt => pt.Position)
                .Select(pt => new PlaylistTrackDto(
                    pt.TrackId,
                    pt.Track.Title,
                    pt.Track.TrackArtists.FirstOrDefault()?.Artist.Name,
                    pt.Position
                )).ToList()
        );
    }
}