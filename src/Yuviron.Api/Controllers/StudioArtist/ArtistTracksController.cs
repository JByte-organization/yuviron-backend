using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;

namespace Yuviron.Api.Controllers.StudioArtist;

[Authorize]
[Route("api/studio-artist/tracks")]
[ApiExplorerSettings(GroupName = "artist")]
public class ArtistTracksController : ApiControllerBase
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTrack([FromRoute] Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteStudioArtistTrackCommand(id), ct);
        return NoContent();
    }
}
