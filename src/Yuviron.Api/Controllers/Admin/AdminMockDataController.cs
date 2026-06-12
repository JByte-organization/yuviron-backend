using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.MockData;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/mock-data")]
public class AdminMockDataController : AdminApiControllerBase
{
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GenerateMockData(
        [FromServices] IServiceScopeFactory scopeFactory,
        [FromServices] ILogger<AdminMockDataController> logger,
        [FromQuery] int months = 6)
    {
        _ = Task.Run(async () =>
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IMockDataService>();
            try
            {
                await service.GenerateAsync(months, CancellationToken.None);
                logger.LogInformation("Mock data generation completed ({Months} months)", months);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Mock data generation failed ({Months} months)", months);
            }
        });

        return Accepted(new { message = $"Mock data generation started for {months} months. Check server logs for progress." });
    }
}
