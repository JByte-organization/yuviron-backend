using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Roles.Queries.GetRoles;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/roles")]
public class AdminRolesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoleDto>>> GetRoles(CancellationToken ct) 
    {
        var result = await Mediator.Send(new GetRolesQuery(), ct);
        return Ok(result); 
    }
}
