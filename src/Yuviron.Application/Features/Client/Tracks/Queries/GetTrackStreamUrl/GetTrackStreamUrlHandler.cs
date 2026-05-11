using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackStreamUrl;

public sealed class GetTrackStreamUrlHandler : IRequestHandler<GetTrackStreamUrlQuery, TrackStreamUrlResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStreamTokenService _streamTokenService;
    private readonly TimeProvider _timeProvider;

    public GetTrackStreamUrlHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStreamTokenService streamTokenService,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _streamTokenService = streamTokenService;
        _timeProvider = timeProvider;
    }

    public async Task<TrackStreamUrlResponse> Handle(GetTrackStreamUrlQuery request, CancellationToken cancellationToken)
    {
        var isAuthenticated = _currentUser.UserId.HasValue;
        
        if (!isAuthenticated)
        {
            throw new UnauthorizedAccessException("You must be logged in to stream audio.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var fileKey = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => t.Id == request.TrackId)
            .Select(t => !string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (fileKey == null)
        {
            throw new NotFoundException(nameof(Track), request.TrackId);
        }

        var audioUrl = _streamTokenService.GenerateAudioUrl(request.TrackId, fileKey, isAuthenticated, _timeProvider);

        if (string.IsNullOrEmpty(audioUrl))
        {
            throw new InvalidOperationException("Failed to generate audio stream URL.");
        }

        return new TrackStreamUrlResponse(audioUrl);
    }
}