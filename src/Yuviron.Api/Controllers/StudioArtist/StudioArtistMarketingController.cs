using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.StudioArtist.Marketing.Commands.SubmitBannerRequest;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/marketing")]
public class StudioArtistMarketingController : StudioArtistApiControllerBase
{
    [HttpPost("banner-requests")]
    [ProducesResponseType(typeof(SubmitBannerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SubmitBannerResponse>> SubmitBannerRequest(
        [FromBody] SubmitBannerRequestCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
}
