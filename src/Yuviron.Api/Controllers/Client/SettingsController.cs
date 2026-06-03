using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Client.Settings.Queries.Commands.RevokeDevice;
using Yuviron.Application.Features.Client.Settings.Queries.GetMyDevices;

namespace Yuviron.Api.Controllers.Client;

[Route("api/me/settigns/devices")]
[Authorize]
public class SettingsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserDeviceDto>>> GetDevices(CancellationToken ct) 
        => Ok(await Mediator.Send(new GetMyDevicesQuery(), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RevokeDevice(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new RevokeDeviceCommand(id), ct);
        return NoContent();
    }
}