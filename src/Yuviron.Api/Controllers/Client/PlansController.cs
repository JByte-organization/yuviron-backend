using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Client.Plans.Queries.GetClientPlans;

namespace Yuviron.Api.Controllers.Client;

[Route("api/plans")]
[ApiExplorerSettings(GroupName = "client")]
public class PlansController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous] 
    [ProducesResponseType(typeof(List<PlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPlansQuery(), ct);
        return Ok(result);
    }
}