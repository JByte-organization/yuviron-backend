using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.System.Commands;
using Yuviron.Application.MockData;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/mock-data")]
public class AdminMockDataController : AdminApiControllerBase
{
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult GenerateMockData([FromQuery] GenerateMockDataCommand command)
    {
        // We run it in background because it is very heavy
        _ = Task.Run(async () =>
        {
            await Mediator.Send(command);
        });

        return Accepted(new { message = "Mock data generation started. Check server logs for progress." });
    }

    [HttpPost("backfill-roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BackfillRoles(CancellationToken ct)
    {
        await Mediator.Send(new BackfillUserRolesCommand(), ct);
        return NoContent();
    }
}
