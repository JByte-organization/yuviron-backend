using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Features.Admin.System.Commands;
using Yuviron.Application.MockData;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/mock-data")]
public class AdminMockDataController : AdminApiControllerBase
{
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult GenerateMockData(
        [FromServices] IServiceScopeFactory scopeFactory,
        [FromQuery] GenerateMockDataCommand command)
    {
        // We run it in background because it is very heavy
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AdminMockDataController>>();

            try
            {
                await mediator.Send(command);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while generating mock data in background.");
            }
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
