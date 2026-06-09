using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Appearance.Commands.DeleteThemePreset;

public sealed class DeleteThemePresetCommandHandler : IRequestHandler<DeleteThemePresetCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;

    public DeleteThemePresetCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IPermissionService permissionService)
    {
        _context = context;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<Unit> Handle(DeleteThemePresetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var hasCustomThemePermission = await _permissionService.HasPermissionAsync(userId, AppPermission.CustomTheme, cancellationToken);

        if (!hasCustomThemePermission)
        {
            throw new ForbiddenException("Theme presets are a premium feature.");
        }

        var settings = await _context.UserSettings.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserSettings), userId);

        var theme = await _context.Themes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Theme), request.Id);

        if (theme.UserId != userId)
        {
            throw new ForbiddenException("You can only delete your own theme presets.");
        }

        if (settings.ThemeId == theme.Id)
        {
            settings.ApplyDesign(null, settings.CustomThemeId, DateTime.UtcNow);
        }

        _context.Themes.Remove(theme);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
