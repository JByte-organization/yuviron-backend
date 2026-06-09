using MediatR;
using System;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateUserSettings;

public class UpdateUserSettingsCommand : IRequest<Unit>
{
    public string ThemeMode { get; set; } = string.Empty;
    public int AudioQualityPreference { get; set; }
    public int CrossfadeMs { get; set; }
    public bool MakePlaylistsPublicByDefault { get; set; }
    public bool ShowFollowers { get; set; }
    public bool ShowActivity { get; set; }
    public bool PrivateSession { get; set; }
}