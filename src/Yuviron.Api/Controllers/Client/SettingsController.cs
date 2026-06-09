using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Settings.Commands.TogglePrivateSession;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateAudioQuality;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateCrossfade;
using Yuviron.Application.Features.Client.Settings.Commands.UpdatePrivacyToggles;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateTheme;
using Yuviron.Application.Features.Client.Settings.Queries.GetUserSettings;

namespace Yuviron.Api.Controllers.Client;

[Route("api/me/settings")]
[Authorize]
public class SettingsController : ApiControllerBase
{
    [HttpGet("preferences")]
    public async Task<ActionResult<UserSettingsDto>> GetUserSettings(CancellationToken ct)
        => Ok(await Mediator.Send(new GetUserSettingsQuery(), ct));

    [HttpPut("theme")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateTheme([FromBody] UpdateThemeCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut("audio/quality")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAudioQuality([FromBody] UpdateAudioQualityCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut("audio/crossfade")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCrossfade([FromBody] UpdateCrossfadeCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut("privacy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePrivacyToggles([FromBody] UpdatePrivacyTogglesCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut("private-session")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> TogglePrivateSession([FromBody] TogglePrivateSessionCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }
}

