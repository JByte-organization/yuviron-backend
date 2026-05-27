using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Roles.Queries.GetRoles;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/roles")]
public class AdminRolesController : AdminApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoleDto>>> GetRoles(CancellationToken ct) 
    {
        var result = await Mediator.Send(new GetRolesQuery(), ct);
        return Ok(result); 
    }
}
