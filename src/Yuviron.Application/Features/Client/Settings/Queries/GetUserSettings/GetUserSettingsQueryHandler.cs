using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Settings.Queries.GetUserSettings;

public class GetUserSettingsQueryHandler : IRequestHandler<GetUserSettingsQuery, UserSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserSettingsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<UserSettingsDto> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var settings = await _context.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);

        if (settings == null)
        {
            throw new NotFoundException(nameof(UserSettings), userId);
        }

        return new UserSettingsDto
        {
            ThemeMode = settings.ThemeMode,
            ThemeId = settings.ThemeId,
            CustomThemeId = settings.CustomThemeId,
            AudioQualityPreference = settings.AudioQualityPreference,
            CrossfadeMs = settings.CrossfadeMs,
            MakePlaylistsPublicByDefault = settings.MakePlaylistsPublicByDefault,
            ShowFollowers = settings.ShowFollowers,
            PrivateSession = settings.PrivateSession
        };
    }
}
