using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Appearance.Queries.GetCustomTheme;

public sealed class GetCustomThemeHandler : IRequestHandler<GetCustomThemeQuery, CustomThemeDto>
{
    private readonly IProfileContext _profileContext;
    private readonly ICurrentUserService _currentUser;

    public GetCustomThemeHandler(IProfileContext profileContext, ICurrentUserService currentUser)
    {
        _profileContext = profileContext;
        _currentUser = currentUser;
    }

    public async Task<CustomThemeDto> Handle(GetCustomThemeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var settings = await _profileContext.UserSettings
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

        var customTheme = await _profileContext.CustomThemes
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
