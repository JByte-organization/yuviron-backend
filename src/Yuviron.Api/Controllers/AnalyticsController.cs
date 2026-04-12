using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Analytics.Commands.RecordTrackPlay;

namespace Yuviron.Api.Controllers;

[Route("api/analytics")]
public class AnalyticsController : ApiControllerBase
{
    [HttpPost("play")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordPlay([FromBody] RecordTrackPlayCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        
        return Ok();
    }
}