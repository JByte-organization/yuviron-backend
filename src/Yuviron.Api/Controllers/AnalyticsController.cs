using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Analytics.Commands.StartTrackPlay;
using Yuviron.Application.Features.Analytics.Commands.CommitTrackPlay;

namespace Yuviron.Api.Controllers;

[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ApiControllerBase
{
    [HttpPost("play/start")]
    [ProducesResponseType(typeof(StartPlayResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartPlay([FromBody] StartPlayRequest request, CancellationToken ct)
    {
        var command = new StartTrackPlayCommand(request.TrackId, UserId);
        var sessionId = await Mediator.Send(command, ct);
        
        return Ok(new StartPlayResponse(sessionId));
    }

    [HttpPost("play/commit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CommitPlay([FromBody] CommitPlayRequest request, CancellationToken ct)
    {
        var countryCode = Request.Headers["CF-IPCountry"].FirstOrDefault();

        var command = new CommitTrackPlayCommand(
            request.PlaySessionId, 
            request.TrackId, 
            request.ArtistId, 
            UserId,
            request.DeviceType,
            request.SourceType,
            request.SourceId,
            countryCode
        );

        await Mediator.Send(command, ct);
        
        return Ok();
    }
}