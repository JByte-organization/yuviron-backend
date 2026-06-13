using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.StudioArtist.Marketing.Commands.SubmitBannerRequest;
using Yuviron.Application.Features.StudioArtist.Marketing.Commands.PayBannerRequest;
using Yuviron.Application.Features.StudioArtist.Marketing.Queries.GetActiveBannerRequest;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/marketing")]
public class StudioArtistMarketingController : StudioArtistApiControllerBase
{
    [HttpGet("banner-requests/active")]
    [ProducesResponseType(typeof(ActiveBannerRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ActiveBannerRequestDto>> GetActiveBannerRequest(
        [FromQuery] Guid artistId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetActiveBannerRequestQuery(artistId), ct);
        if (result == null) return NoContent();
        return Ok(result);
    }

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

    [HttpPost("banner-requests/{requestId:guid}/pay")]
    [ProducesResponseType(typeof(PayBannerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PayBannerResponse>> PayBannerRequest(
        [FromRoute] Guid requestId,
        [FromBody] PayBannerRequestDto dto,
        CancellationToken ct)
    {
        var command = new PayBannerRequestCommand(requestId, dto.ArtistId, dto.SuccessUrl, dto.CancelUrl);
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
}

public record PayBannerRequestDto(Guid ArtistId, string SuccessUrl, string CancelUrl);
