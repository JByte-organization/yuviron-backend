using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.StudioArtist.Dashboard.Queries.GetDashboardStats;

namespace Yuviron.Api.Controllers.StudioArtist;

[Authorize]
[Route("api/studio-artist")]
[ApiExplorerSettings(GroupName = "artist")]
public class StudioArtistController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(StudioArtistDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudioArtistDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(new GetStudioArtistDashboardQuery(), cancellationToken));
    }
}
