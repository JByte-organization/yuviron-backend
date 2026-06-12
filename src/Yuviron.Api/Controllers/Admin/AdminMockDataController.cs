using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.MockData;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/mock-data")]
public class AdminMockDataController : AdminApiControllerBase
{
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateMockData([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        await Mediator.Send(new GenerateMockDataCommand(months), cancellationToken);
        return NoContent();
    }
}
