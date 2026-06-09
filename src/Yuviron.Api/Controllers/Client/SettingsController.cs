using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Settings.Queries.Commands.RevokeDevice;
using Yuviron.Application.Features.Client.Settings.Queries.GetMyDevices;
using Yuviron.Application.Features.Client.Settings.Queries.GetUserSettings;
using Yuviron.Application.Features.Client.Settings.Commands.UpdateUserSettings;

namespace Yuviron.Api.Controllers.Client;

[Route("api/me/settings")]
[Authorize]
public class SettingsController : ApiControllerBase
{
    [HttpGet("devices")]
    public async Task<ActionResult<List<UserDeviceDto>>> GetDevices(CancellationToken ct) 
        => Ok(await Mediator.Send(new GetMyDevicesQuery(), ct));

    [HttpDelete("devices/{id:guid}")]
    public async Task<IActionResult> RevokeDevice(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new RevokeDeviceCommand(id), ct);
        return NoContent();
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<UserSettingsDto>> GetUserSettings(CancellationToken ct)
        => Ok(await Mediator.Send(new GetUserSettingsQuery(), ct));

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdateUserSettings([FromBody] UpdateUserSettingsCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }
}
