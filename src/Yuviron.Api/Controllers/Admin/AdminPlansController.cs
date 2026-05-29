using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Plans.Commands.CreatePlan;
using Yuviron.Application.Features.Admin.Plans.Commands.DeletePlan;
using Yuviron.Application.Features.Admin.Plans.Commands.UpdatePlan;
using Yuviron.Application.Features.Admin.Plans.Queries.GetPlanById;
using Yuviron.Application.Features.Admin.Plans.Queries.GetPlans;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/plans")]
[ApiExplorerSettings(GroupName = "admin")]
public class AdminPlansController : AdminApiControllerBase 
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<PlanListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans([FromQuery] GetPlansQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlanDetailsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlanById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPlanByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)] 
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanCommand command, CancellationToken ct)
    {
        var planId = await Mediator.Send(command, ct);
        return Ok(planId); 
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdatePlanCommand command, CancellationToken ct)
    {
        var commandWithId = command with { Id = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePlan(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeletePlanCommand(id), ct);
        return NoContent();
    }
}