using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Auth.Commands.ChangePassword;
using Yuviron.Application.Features.Auth.Commands.LogoutEverywhere;
using Yuviron.Application.Features.Client.Settings.Queries.Commands.RevokeDevice;
using Yuviron.Application.Features.Client.Settings.Queries.GetMyDevices;

namespace Yuviron.Api.Controllers.Client;

[Authorize]
[Route("api/me/security")]
[ApiExplorerSettings(GroupName = "client")]
public class SecurityController : ApiControllerBase
{
    [HttpGet("devices")]
    [ProducesResponseType(typeof(List<UserDeviceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserDeviceDto>>> GetDevices(CancellationToken ct)
        => Ok(await Mediator.Send(new GetMyDevicesQuery(), ct));

    [HttpDelete("devices/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeDevice([FromRoute] Guid id, CancellationToken ct)
    {
        await Mediator.Send(new RevokeDeviceCommand(id), ct);
        return NoContent();
    }

    [HttpPost("logout-everywhere")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutEverywhere(CancellationToken ct)
    {
        await Mediator.Send(new LogoutEverywhereCommand(), ct);
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
        return NoContent();
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }
}
