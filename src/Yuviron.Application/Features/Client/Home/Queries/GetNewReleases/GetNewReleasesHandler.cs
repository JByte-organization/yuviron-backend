using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
using Yuviron.Application.Extensions;
using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Home.Queries.GetNewReleases;

public sealed class GetNewReleasesHandler : IRequestHandler<GetNewReleasesQuery, List<NewReleaseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider; 

    public GetNewReleasesHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<NewReleaseDto>> Handle(GetNewReleasesQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        return await _context.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(a => a.Tracks.Any(t => !t.IsDeleted 
                                          && t.VisibilityStatus == VisibilityStatus.Published 
                                          && t.ProcessingStatus == TrackProcessingStatus.Ready))
            .OrderByDescending(a => a.ReleaseDate) 
            .ThenByDescending(a => a.CreatedAt)
            .Take(request.Limit)
            .Select(a => new NewReleaseDto(
                a.Id,
                a.Title,
                a.AlbumArtists.Select(aa => new TrackArtistDto(aa.Artist.Id, aa.Artist.Name, aa.Role)),
                a.CoverUrl,
                a.Tracks.Count(t => !t.IsDeleted 
                                    && t.VisibilityStatus == VisibilityStatus.Published
                                    && t.ProcessingStatus == TrackProcessingStatus.Ready) 
            ))
            .ToListAsync(cancellationToken);
    }
}