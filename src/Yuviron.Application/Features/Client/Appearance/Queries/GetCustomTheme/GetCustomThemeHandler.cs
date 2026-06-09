using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Appearance.Queries.GetCustomTheme;

public sealed class GetCustomThemeHandler : IRequestHandler<GetCustomThemeQuery, CustomThemeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCustomThemeHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CustomThemeDto> Handle(GetCustomThemeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var settings = await _context.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (settings == null)
        {
            throw new NotFoundException(nameof(UserSettings), userId);
        }

        if (!settings.CustomThemeId.HasValue)
        {
            throw new NotFoundException(nameof(CustomTheme), userId);
        }

        var customTheme = await _context.CustomThemes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == settings.CustomThemeId.Value, cancellationToken);

        if (customTheme == null)
        {
            throw new NotFoundException(nameof(CustomTheme), settings.CustomThemeId.Value);
        }

        return new CustomThemeDto(
            customTheme.Id,
            customTheme.PrimaryColor,
            customTheme.SecondaryColor,
            customTheme.BackgroundColor,
            customTheme.CreatedAt);
    }
}
