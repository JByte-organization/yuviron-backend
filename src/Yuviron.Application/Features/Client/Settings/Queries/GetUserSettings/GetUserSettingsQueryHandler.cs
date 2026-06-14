using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly IProfileContext _profileContext;
    private readonly ICurrentUserService _currentUser;

    public GetUserSettingsQueryHandler(IProfileContext profileContext, ICurrentUserService currentUser)
    {
        _profileContext = profileContext;
        _currentUser = currentUser;
    }

    public async Task<UserSettingsDto> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var settings = await _profileContext.UserSettings
            .FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);

        if (settings == null)
        {
            settings = UserSettings.Create(userId, DateTime.UtcNow);
            _profileContext.Add(settings);
            await _profileContext.SaveChangesAsync(cancellationToken);
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
