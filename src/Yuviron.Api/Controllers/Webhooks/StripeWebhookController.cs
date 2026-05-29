using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Api.Controllers.Webhooks;

[AllowAnonymous] 
[Route("api/webhooks/stripe")]
[ApiExplorerSettings(IgnoreApi = true)] 
public class StripeWebhookController : ControllerBase
{
    private readonly IStripeWebhookParser _webhookParser;

    public StripeWebhookController(IStripeWebhookParser webhookParser)
    {
        _webhookParser = webhookParser;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook(CancellationToken ct)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        await _webhookParser.ProcessWebhookAsync(json, signature, ct);

        return Ok(); 
    }
}