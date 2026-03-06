using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTrackById;

public sealed class GetTrackByIdHandler : IRequestHandler<GetTrackByIdQuery, TrackDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetTrackByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TrackDetailsDto> Handle(GetTrackByIdQuery request, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
            .AsNoTracking()
            .IgnoreQueryFilters() 
            .Where(t => t.Id == request.TrackId)
            .Select(t => new TrackDetailsDto(
                t.Id,
                t.AlbumId,
                t.Title,
                t.DurationMs,
                t.Explicit,
                t.CoverUrl,
                t.AudioStorageKey,
                t.PreviewStorageKey,
                t.VisibilityStatus,
                t.IsDeleted,
                t.CreatedAt,
                t.UpdatedAt,
                t.TrackArtists.Select(ta => ta.ArtistId).ToList(), // Вытаскиваем только ID артистов
                t.TrackGenres.Select(tg => tg.GenreId).ToList(),
                t.TrackMoods.Select(tg => tg.MoodId).ToList() // Вытаскиваем только ID жанров
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (track is null)
        {
            throw new NotFoundException(nameof(Track), request.TrackId);
        }

        return track;
    }
}