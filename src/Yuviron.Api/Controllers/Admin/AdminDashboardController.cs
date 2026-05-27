using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Dashboard.Queries.GetDashboardStats;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/dashboard")]
public class AdminDashboardController : AdminApiControllerBase
{
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminDashboardDto>> GetStats(CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(new GetAdminDashboardQuery(), cancellationToken));
    }
}