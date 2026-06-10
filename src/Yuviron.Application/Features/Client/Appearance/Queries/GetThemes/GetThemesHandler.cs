using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Appearance.Queries.GetThemes;

public sealed class GetThemesHandler : IRequestHandler<GetThemesQuery, List<ThemeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;

    public GetThemesHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IPermissionService permissionService)
    {
        _context = context;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<List<ThemeDto>> Handle(GetThemesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var hasCustomThemePermission = await _permissionService.HasPermissionAsync(
            userId,
            AppPermission.CustomTheme,
            cancellationToken);

        var settings = await _context.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (settings == null)
        {
            throw new NotFoundException(nameof(UserSettings), userId);
        }

        var currentThemeId = settings.ThemeId;

        return await _context.Themes
            .AsNoTracking()
            .Where(x => x.UserId == null || x.UserId == userId)
            .Where(x => !x.IsPremiumOnly || hasCustomThemePermission)
            .OrderByDescending(x => x.IsSystem)
            .ThenBy(x => x.UserId.HasValue ? 1 : 0)
            .ThenBy(x => x.Name)
            .Select(x => new ThemeDto(
                x.Id,
                x.Name,
                x.PrimaryColor,
                x.SecondaryColor,
                x.BackgroundColor,
                x.IsSystem,
                x.IsPremiumOnly,
                x.UserId == userId,
                currentThemeId.HasValue && currentThemeId.Value == x.Id))
            .ToListAsync(cancellationToken);
    }
}
