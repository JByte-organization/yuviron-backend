using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Application.Policies; // <-- Подключили конкретный класс политики
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackStreamUrl;

public sealed class GetTrackStreamUrlHandler : IRequestHandler<GetTrackStreamUrlQuery, TrackStreamUrlResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStreamTokenService _streamTokenService;
    private readonly TimeProvider _timeProvider;
    private readonly UserSettingsPolicy _settingsPolicy; // <-- Убрали букву "I"
    private readonly IPermissionService _permissionService; 

    public GetTrackStreamUrlHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStreamTokenService streamTokenService,
        TimeProvider timeProvider,
        UserSettingsPolicy settingsPolicy, // <-- Инжектим класс напрямую
        IPermissionService permissionService) 
    {
        _context = context;
        _currentUser = currentUser;
        _streamTokenService = streamTokenService;
        _timeProvider = timeProvider;
        _settingsPolicy = settingsPolicy;
        _permissionService = permissionService;
    }

    public async Task<TrackStreamUrlResponse> Handle(GetTrackStreamUrlQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId 
            ?? throw new UnauthorizedAccessException("You must be logged in to stream audio.");

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

        bool hasHighQuality = await _permissionService.CanStreamHighQualityAudioAsync(currentUserId, cancellationToken);

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Settings)
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken); 

        if (user == null)
        {
            throw new UnauthorizedAccessException("User account not found.");
        }

        int targetQuality = _settingsPolicy.GetAllowedStreamQuality(user.Settings, hasHighQuality);

        var audioUrl = _streamTokenService.GenerateAudioUrl(
            request.TrackId, 
            targetQuality, 
            fileKey, 
            true, 
            _timeProvider);

        if (string.IsNullOrEmpty(audioUrl))
        {
            throw new InvalidOperationException("Failed to generate audio stream URL.");
        }

        return new TrackStreamUrlResponse(audioUrl);
    }
}