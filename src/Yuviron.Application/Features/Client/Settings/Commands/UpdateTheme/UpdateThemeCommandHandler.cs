using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Policies;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateTheme;

public class UpdateThemeCommandHandler : IRequestHandler<UpdateThemeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;
    private readonly UserSettingsPolicy _policy;

    public UpdateThemeCommandHandler(
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

    public async Task<Unit> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var hasCustomThemePermission = await _permissionService.HasPermissionAsync(userId, AppPermission.CustomTheme, cancellationToken);

        if (!_policy.CanUseThemeMode(hasCustomThemePermission, request.ThemeMode))
        {
            throw new ForbiddenException("System theme mode is a premium feature.");
        }

        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);
        if (settings == null) throw new NotFoundException(nameof(UserSettings), userId);

        settings.UpdateTheme(request.ThemeMode, DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
