using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Themes.Commands.CreateTheme;
using Yuviron.Application.Features.Admin.Themes.Commands.DeleteTheme;
using Yuviron.Application.Features.Admin.Themes.Commands.UpdateTheme;
using Yuviron.Application.Features.Admin.Themes.Queries.DTOs;
using Yuviron.Application.Features.Admin.Themes.Queries.GetThemeById;
using Yuviron.Application.Features.Admin.Themes.Queries.GetThemes;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/themes")]
public class AdminThemesController : AdminApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ThemeListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<ThemeListItemDto>>> GetThemes([FromQuery] GetThemesQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ThemeDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ThemeDetailsDto>> GetThemeById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetThemeByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateThemeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateThemeResponse>> CreateTheme([FromBody] CreateThemeCommand command, CancellationToken ct)
    {
        var themeId = await Mediator.Send(command, ct);
        return Ok(new CreateThemeResponse(themeId));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateTheme(Guid id, [FromBody] UpdateThemeCommand command, CancellationToken ct)
    {
        var commandWithId = command with { ThemeId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTheme(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteThemeCommand(id), ct);
        return NoContent();
    }
}
