using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Policies;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateCustomTheme;

public sealed class UpdateCustomThemeCommandHandler : IRequestHandler<UpdateCustomThemeCommand, Unit>
{
    private readonly IProfileContext _profileContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;
    private readonly UserSettingsPolicy _policy;

    public UpdateCustomThemeCommandHandler(
        IProfileContext profileContext,
        ICurrentUserService currentUser,
        IPermissionService permissionService,
        UserSettingsPolicy policy)
    {
        _profileContext = profileContext;
        _currentUser = currentUser;
        _permissionService = permissionService;
        _policy = policy;
    }

    public async Task<Unit> Handle(UpdateCustomThemeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var settings = await _profileContext.UserSettings
            .FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);

        if (settings == null)
        {
            throw new NotFoundException(nameof(UserSettings), userId);
        }

        var hasCustomThemePermission = await _permissionService.HasPermissionAsync(
            userId, AppPermission.CustomTheme, cancellationToken);

        if (!_policy.CanUseCustomTheme(hasCustomThemePermission))
        {
            throw new ForbiddenException("Custom themes are a premium feature.");
        }

        var utcNow = DateTime.UtcNow;

        var customTheme = await _profileContext.CustomThemes
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (customTheme == null)
        {
            customTheme = CustomTheme.Create(
                userId,
                request.PrimaryColor,
                request.SecondaryColor,
                request.BackgroundColor,
                utcNow);

            _profileContext.Add(customTheme);
        }
        else
        {
            customTheme.Update(
                request.PrimaryColor,
                request.SecondaryColor,
                request.BackgroundColor);
        }

        settings.ApplyDesign(settings.ThemeId, customTheme.Id, utcNow);

        await _profileContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
