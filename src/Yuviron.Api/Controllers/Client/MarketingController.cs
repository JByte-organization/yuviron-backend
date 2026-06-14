using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Client.Marketing.Commands.CreateSmartLink;

namespace Yuviron.Api.Controllers.Client;

[Authorize]
[Route("api/marketing")]
public class MarketingController : ApiControllerBase
{
    [HttpPost("smartlinks")]
    [ProducesResponseType(typeof(SmartLinkDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SmartLinkDto>> CreateSmartLink([FromBody] CreateSmartLinkCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
}
