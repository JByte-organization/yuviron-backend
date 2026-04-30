using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services; // <-- Для ICurrentUserService
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackById;

public sealed class GetTrackByIdHandler : IRequestHandler<GetTrackByIdQuery, TrackDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser; 

    public GetTrackByIdHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TrackDetailsDto> Handle(GetTrackByIdQuery request, CancellationToken cancellationToken)
    {
        bool isAuthenticated = _currentUser.UserId.HasValue;

        var trackDto = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == request.Id 
                     && !t.IsDeleted 
                     && t.VisibilityStatus == VisibilityStatus.Published
                     && t.ProcessingStatus == TrackProcessingStatus.Ready)
            .Select(t => new TrackDetailsDto(
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                
                isAuthenticated 
                    ? (!string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey) 
                    : null, 
                    
                t.PlayCount,
                t.AlbumId,
                t.Album != null ? t.Album.Title : "Unknown Album",
                t.AlbumPosition,
                t.TrackArtists.Select(ta => new TrackArtistDto(
                    ta.Artist.Id,
                    ta.Artist.Name,
                    ta.Role.ToString() 
                )).ToList(),
                t.TrackGenres.Select(tg => tg.Genre.Name).ToList(),
                t.TrackMoods.Select(tm => tm.Mood.Name).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (trackDto == null)
        {
            throw new NotFoundException(nameof(Track), request.Id);
        }

        return trackDto;
    }
}