using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/jamendo")]
public class AdminJamendoController : ApiControllerBase
{
    [HttpPost("sync")]
    [ProducesResponseType(typeof(SyncJamendoResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SyncJamendoResponse>> SyncTracks([FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var command = new SyncJamendoTracksCommand(limit);
        
        var syncedCount = await Mediator.Send(command, ct);

        return Ok(new SyncJamendoResponse("Синхронизация завершена", syncedCount));
    }
}

public record SyncJamendoResponse(string Message, int SyncedTracksCount);