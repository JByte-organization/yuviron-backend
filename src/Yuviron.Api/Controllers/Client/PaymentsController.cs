using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Payments.Commands.CreateCheckoutSession;
using Yuviron.Application.Features.StudioArtist.Payments.Commands.CancelSubscription;

namespace Yuviron.Api.Controllers.Client;

[Authorize]
[Route("api/payments")]
[ApiExplorerSettings(GroupName = "client")]
public class PaymentsController : ApiControllerBase
{
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCheckout([FromBody] CreateCheckoutRequest request, CancellationToken ct)
    {
        var command = new CreateCheckoutSessionCommand(request.PlanId, request.SuccessUrl, request.CancelUrl);
        var checkoutUrl = await Mediator.Send(command, ct);
        
        return Ok(new { url = checkoutUrl });
    }

    [HttpPost("cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelSubscription(CancellationToken ct)
    {
        await Mediator.Send(new CancelSubscriptionCommand(), ct);
        return Ok(new { message = "Subscription will not auto-renew. You keep Premium until the end of your billing period." });
    }
}

public record CreateCheckoutRequest(Guid PlanId, string SuccessUrl, string CancelUrl);