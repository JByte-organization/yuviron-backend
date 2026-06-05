using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTrackLyrics;

public sealed class GetStudioTrackLyricsHandler : IRequestHandler<GetStudioTrackLyricsQuery, StudioTrackLyricsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetStudioTrackLyricsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<StudioTrackLyricsDto> Handle(GetStudioTrackLyricsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var trackData = await _context.Tracks
                            .AsNoTracking()
                            .Where(t => t.Id == request.TrackId && !t.IsDeleted)
                            .Select(t => new
                            {
                                ArtistIds = t.TrackArtists.Select(ta => ta.ArtistId).ToList(),
                                LyricsText = t.Lyrics != null ? t.Lyrics.PlainText : null 
                            })
                            .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var hasAccess = await _context.ArtistTeamMembers
            .HasManagementAccess(trackData.ArtistIds, userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this track.");

        return new StudioTrackLyricsDto(trackData.LyricsText);
    }
}