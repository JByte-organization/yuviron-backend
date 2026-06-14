using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
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
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;

    public GetStudioTrackLyricsHandler(ICatalogContext catalogContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _currentUser = currentUser;
    }

    public async Task<StudioTrackLyricsDto> Handle(GetStudioTrackLyricsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var trackData = await _catalogContext.Tracks
                            .AsNoTracking()
                            .Where(t => t.Id == request.TrackId && !t.IsDeleted)
                            .Select(t => new
                            {
                                ArtistIds = t.TrackArtists.Select(ta => ta.ArtistId).ToList(),
                                LyricsText = t.Lyrics != null ? t.Lyrics.PlainText : null 
                            })
                            .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new NotFoundException(nameof(Track), request.TrackId);

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .Where(tm => trackData.ArtistIds.Contains(tm.ArtistId) && tm.UserId == userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this track.");

        return new StudioTrackLyricsDto(trackData.LyricsText);
    }
}
