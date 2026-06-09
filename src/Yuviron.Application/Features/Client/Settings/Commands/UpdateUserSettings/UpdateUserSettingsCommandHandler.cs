using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Policies;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateUserSettings;

public class UpdateUserSettingsCommandHandler : IRequestHandler<UpdateUserSettingsCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;
    private readonly UserSettingsPolicy _policy;

    public UpdateUserSettingsCommandHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser,
        IPermissionService permissionService,
        UserSettingsPolicy policy)
    {
        _context = context;
        _currentUser = currentUser;
        _permissionService = permissionService;
        _policy = policy;
    }

    public async Task<Unit> Handle(UpdateUserSettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);

        if (settings == null)
        {
            throw new NotFoundException(nameof(UserSettings), userId);
        }

        bool hasHighQuality = await _permissionService.HasPermissionAsync(userId, AppPermission.PlayerHighQuality, cancellationToken);
        bool hasPrivateSession = await _permissionService.HasPermissionAsync(userId, AppPermission.PrivateSession, cancellationToken);

        int sanitizedQuality = _policy.SanitizeAudioQuality(hasHighQuality, request.AudioQualityPreference);
        bool canUsePrivateSession = _policy.CanUsePrivateSession(hasPrivateSession, request.PrivateSession);

        if (request.PrivateSession && !canUsePrivateSession)
        {
            throw new ForbiddenException("Private session is a premium feature.");
        }

        settings.UpdatePreferences(
            request.ThemeMode,
            sanitizedQuality,
            request.CrossfadeMs,
            request.MakePlaylistsPublicByDefault,
            request.ShowFollowers,
            request.ShowActivity,
            request.PrivateSession && canUsePrivateSession,
            DateTime.UtcNow
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
