using MediatR;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Api.Controllers;
using Yuviron.Application.Features.Admin.System.Commands;
using System.Threading.Tasks;

namespace Yuviron.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/system")]
public class SystemController : ApiControllerBase
{
    private readonly ISender _sender;

    public SystemController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("backfill-roles")]
    public async Task<IActionResult> BackfillRoles()
    {
        await _sender.Send(new BackfillUserRolesCommand());
        return Ok();
    }
}

