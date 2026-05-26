using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Ads.Commands.RegisterClick;
using Yuviron.Application.Features.Client.Ads.Commands.RegisterImpression;

namespace Yuviron.Api.Controllers.Client;

[Route("api/ads")]
[ApiExplorerSettings(GroupName = "client")]
[Authorize]
public class AdsController : ApiControllerBase
{
    [HttpPost("{id:guid}/impressions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RegisterImpression(
        [FromRoute] Guid id, 
        [FromBody] RegisterAdImpressionRequest request, 
        CancellationToken ct)
    {
        await Mediator.Send(new RegisterAdImpressionCommand(id, request.Context), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/clicks")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RegisterClick([FromRoute] Guid id, CancellationToken ct)
    {
        await Mediator.Send(new RegisterAdClickCommand(id), ct);
        return NoContent();
    }
}

public record RegisterAdImpressionRequest(string? Context);