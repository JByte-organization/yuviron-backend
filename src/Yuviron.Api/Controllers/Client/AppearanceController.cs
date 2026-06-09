using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Client.Appearance.Commands.ActivateThemePreset;
using Yuviron.Application.Features.Client.Appearance.Commands.CreateThemePreset;
using Yuviron.Application.Features.Client.Appearance.Commands.DeleteThemePreset;
using Yuviron.Application.Features.Client.Appearance.Commands.UpdateThemePreset;
using Yuviron.Application.Features.Client.Appearance.Queries.GetCustomTheme;
using Yuviron.Application.Features.Client.Appearance.Queries.GetThemes;
using Yuviron.Application.Features.Client.Appearance.Queries.GetThemeModes;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateCustomTheme;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateTheme;
using Yuviron.Domain.Enums;

namespace Yuviron.Api.Controllers.Client;

[Authorize]
[Route("api/me/appearance")]
[ApiExplorerSettings(GroupName = "client")]
public class AppearanceController : ApiControllerBase
{
    [HttpGet("modes")]
    [ProducesResponseType(typeof(List<ThemeMode>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ThemeMode>>> GetThemeModes(CancellationToken ct)
        => Ok(await Mediator.Send(new GetThemeModesQuery(), ct));

    [HttpPut("mode")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateThemeMode([FromBody] UpdateThemeCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut("custom-theme")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCustomTheme([FromBody] UpdateCustomThemeCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpGet("themes")]
    [ProducesResponseType(typeof(List<ThemeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ThemeDto>>> GetThemes(CancellationToken ct)
        => Ok(await Mediator.Send(new GetThemesQuery(), ct));

    [HttpPost("themes")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> CreateThemePreset([FromBody] CreateThemePresetCommand command, CancellationToken ct)
        => Ok(await Mediator.Send(command, ct));

    [HttpPut("themes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateThemePreset(Guid id, [FromBody] UpdateThemePresetCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpDelete("themes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteThemePreset(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteThemePresetCommand(id), ct);
        return NoContent();
    }

    [HttpPut("themes/{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivateThemePreset(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new ActivateThemePresetCommand(id), ct);
        return NoContent();
    }

    [HttpGet("custom-theme")]
    [ProducesResponseType(typeof(CustomThemeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomThemeDto>> GetCustomTheme(CancellationToken ct)
        => Ok(await Mediator.Send(new GetCustomThemeQuery(), ct));
}
