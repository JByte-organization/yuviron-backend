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

namespace Yuviron.Application.Features.Client.Appearance.Commands.CreateThemePreset;

public sealed class CreateThemePresetCommandHandler : IRequestHandler<CreateThemePresetCommand, Guid>
{
    private readonly IProfileContext _profileContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;

    public CreateThemePresetCommandHandler(
        IProfileContext profileContext,
        ICurrentUserService currentUser,
        IPermissionService permissionService)
    {
        _profileContext = profileContext;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<Guid> Handle(CreateThemePresetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var hasCustomThemePermission = await _permissionService.HasPermissionAsync(userId, AppPermission.CustomTheme, cancellationToken);

        if (!hasCustomThemePermission)
        {
            throw new ForbiddenException("Theme presets are a premium feature.");
        }

        var settings = await _profileContext.UserSettings.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserSettings), userId);

        var normalizedName = request.Name.Trim();
        var utcNow = DateTime.UtcNow;

        var existing = await _profileContext.Themes
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Name == normalizedName, cancellationToken);

        if (existing == null)
        {
            existing = Theme.Create(
                normalizedName,
                request.PrimaryColor,
                request.SecondaryColor,
                request.BackgroundColor,
                isSystem: false,
                isPremiumOnly: true,
                userId: userId);

            _profileContext.Add(existing);
        }
        else
        {
            existing.Update(
                normalizedName,
                request.PrimaryColor,
                request.SecondaryColor,
                request.BackgroundColor,
                isSystem: false,
                isPremiumOnly: true);
        }

        settings.ApplyDesign(existing.Id, null, utcNow);
        await _profileContext.SaveChangesAsync(cancellationToken);

        return existing.Id;
    }
}
