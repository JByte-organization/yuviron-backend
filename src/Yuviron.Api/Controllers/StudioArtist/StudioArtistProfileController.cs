using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.StudioArtist.Profile.Queries.GetStudioArtistProfile;
using Yuviron.Application.Features.StudioArtist.Artists.Commands.UpdateArtist;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/profile")]
public class StudioArtistProfileController : StudioArtistApiControllerBase
{
    [HttpGet("{artistId:guid}")]
    [ProducesResponseType(typeof(StudioArtistProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudioArtistProfileDto>> GetProfile(Guid artistId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetStudioArtistProfileQuery(artistId), ct);
        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateArtist(Guid id, [FromBody] UpdateArtistCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { ArtistId = id }, ct);
        return NoContent();
    }
}