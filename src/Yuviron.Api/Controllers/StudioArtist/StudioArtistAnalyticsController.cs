using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetStudioArtistStats;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist")] 
public class StudioArtistAnalyticsController : StudioArtistApiControllerBase
{
    [HttpGet("stats")]
    [ProducesResponseType(typeof(StudioArtistStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudioArtistStatsDto>> GetStats([FromQuery] Guid artistId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetStudioArtistStatsQuery(artistId), ct);
        return Ok(result);
    }
}