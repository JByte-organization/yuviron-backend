using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options; // <-- Добавили для IOptions
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration; 
using Yuviron.Application.Extensions;
using Yuviron.Application.Policies;
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
    private readonly UserSettingsPolicy _settingsPolicy;
    private readonly IPermissionService _permissionService;
    
    private readonly int _adCooldownMinutes; 

    public GetTrackStreamUrlHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStreamTokenService streamTokenService,
        TimeProvider timeProvider,
        UserSettingsPolicy settingsPolicy,
        IPermissionService permissionService,
        IOptions<AdSettingsOptions> adOptions) 
    {
        _context = context;
        _currentUser = currentUser;
        _streamTokenService = streamTokenService;
        _timeProvider = timeProvider;
        _settingsPolicy = settingsPolicy;
        _permissionService = permissionService;
        
        _adCooldownMinutes = adOptions.Value.CooldownMinutes; 
    }

    public async Task<TrackStreamUrlResponse> Handle(GetTrackStreamUrlQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("You must be logged in.");
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var fileKey = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => t.Id == request.TrackId)
            .Select(t => !string.IsNullOrWhiteSpace(t.HlsPlaylistUrl) ? t.HlsPlaylistUrl : t.AudioStorageKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (fileKey == null) throw new NotFoundException(nameof(Track), request.TrackId);

        bool hasHighQuality = await _permissionService.HasPermissionAsync(currentUserId, AppPermission.PlayerHighQuality, cancellationToken);
        bool hasNoAds = await _permissionService.HasPermissionAsync(currentUserId, AppPermission.PlayerNoAds, cancellationToken);

        var user = await _context.Users.AsNoTracking().Include(u => u.Settings).FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken); 
        if (user == null) throw new UnauthorizedAccessException();

        int targetQuality = _settingsPolicy.GetAllowedStreamQuality(user.Settings, hasHighQuality);
        var audioUrl = _streamTokenService.GenerateAudioUrl(request.TrackId, targetQuality, fileKey, true, _timeProvider);
        
        if (string.IsNullOrEmpty(audioUrl))
        {
            throw new InvalidOperationException("Failed to generate stream URL.");
        }

        AdPlaybackDto? pendingAd = null;
        if (!hasNoAds)
        {
            var adResult = await _context.GetAdIfCooldownPassedAsync(
                currentUserId, 
                utcNow, 
                _adCooldownMinutes, 
                cancellationToken);

            if (adResult != null)
            {
                pendingAd = new AdPlaybackDto(
                    adResult.AdId,
                    adResult.AudioUrl,
                    adResult.ImageUrl,
                    adResult.AdvertiserName,
                    adResult.Title,
                    adResult.ClickUrl
                );
            }
        }

        return new TrackStreamUrlResponse(audioUrl, pendingAd);
    }
    
}