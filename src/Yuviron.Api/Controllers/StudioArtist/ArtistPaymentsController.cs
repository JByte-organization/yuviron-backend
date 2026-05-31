using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Payments.Commands.CreateArtistCheckoutSession;
using Yuviron.Application.Features.StudioArtist.Payments.Commands.CancelArtistSubscription;

namespace Yuviron.Api.Controllers.StudioArtist;

[Authorize]
[Route("api/studio-artist/payments")]
[ApiExplorerSettings(GroupName = "artist")]
public class ArtistPaymentsController : ApiControllerBase
{
    [HttpPost("artist-checkout")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateArtistCheckout([FromBody] CreateArtistCheckoutRequest request, CancellationToken ct)
    {
        var command = new CreateArtistCheckoutSessionCommand(request.ArtistId, request.PlanId, request.SuccessUrl, request.CancelUrl);
        var checkoutUrl = await Mediator.Send(command, ct);
        
        return Ok(new { url = checkoutUrl });
    }

    [HttpPost("cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelArtistSubscription([FromBody] CancelArtistRequest request, CancellationToken ct)
    {
        await Mediator.Send(new CancelArtistSubscriptionCommand(request.ArtistId), ct);
        return Ok(new { message = "Artist Pro subscription will not auto-renew. You keep benefits until the end of the billing period." });
    }
    
    public record CreateArtistCheckoutRequest(Guid ArtistId, Guid PlanId, string SuccessUrl, string CancelUrl);
    public record CancelArtistRequest(Guid ArtistId);
}