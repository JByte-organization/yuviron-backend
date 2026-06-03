using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.StudioArtist.Profile.Queries.GetStudioArtistProfile;

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
}