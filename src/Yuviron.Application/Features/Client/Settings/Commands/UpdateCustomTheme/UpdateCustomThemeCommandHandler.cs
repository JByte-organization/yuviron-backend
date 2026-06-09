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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;
    private readonly UserSettingsPolicy _policy;

    public UpdateCustomThemeCommandHandler(
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

    public async Task<Unit> Handle(UpdateCustomThemeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var settings = await _context.UserSettings
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

        var customTheme = await _context.CustomThemes
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (customTheme == null)
        {
            customTheme = CustomTheme.Create(
                userId,
                request.PrimaryColor,
                request.SecondaryColor,
                request.BackgroundColor,
                utcNow);

            _context.CustomThemes.Add(customTheme);
        }
        else
        {
            customTheme.Update(
                request.PrimaryColor,
                request.SecondaryColor,
                request.BackgroundColor);
        }

        settings.ApplyDesign(settings.ThemeId, customTheme.Id, utcNow);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
