using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Appearance.Commands.UpdateThemePreset;

public sealed class UpdateThemePresetCommandHandler : IRequestHandler<UpdateThemePresetCommand, Unit>
{
    private readonly IProfileContext _profileContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;

    public UpdateThemePresetCommandHandler(
        IProfileContext profileContext,
        ICurrentUserService currentUser,
        IPermissionService permissionService)
    {
        _profileContext = profileContext;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<Unit> Handle(UpdateThemePresetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var hasCustomThemePermission = await _permissionService.HasPermissionAsync(userId, AppPermission.CustomTheme, cancellationToken);

        if (!hasCustomThemePermission)
        {
            throw new ForbiddenException("Theme presets are a premium feature.");
        }

        var settings = await _profileContext.UserSettings.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserSettings), userId);

        var theme = await _profileContext.Themes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Theme), request.Id);

        if (theme.UserId != userId)
        {
            throw new ForbiddenException("You can only edit your own theme presets.");
        }

        theme.Update(
            request.Name.Trim(),
            request.PrimaryColor,
            request.SecondaryColor,
            request.BackgroundColor,
            isSystem: false,
            isPremiumOnly: true,
            userId: userId);

        settings.ApplyDesign(theme.Id, null, DateTime.UtcNow);
        await _profileContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
