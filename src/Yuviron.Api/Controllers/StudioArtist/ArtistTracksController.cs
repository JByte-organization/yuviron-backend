using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetTracks;

namespace Yuviron.Api.Controllers.StudioArtist;

[Authorize]
[Route("api/studio-artist/tracks")]
[ApiExplorerSettings(GroupName = "artist")]
public class ArtistTracksController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<StudioArtistTrackListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<StudioArtistTrackListItemDto>>> GetTracks(
        [FromQuery] GetStudioArtistTracksQuery query,
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
}
