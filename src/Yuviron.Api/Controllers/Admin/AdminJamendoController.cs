using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/jamendo")]
public class AdminJamendoController : AdminApiControllerBase
{
    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult SyncTracks(
        [FromServices] IServiceScopeFactory scopeFactory,
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0)
    {
        var adminId = UserId;
        var command = new SyncJamendoTracksCommand(limit, offset);

        _ = Task.Run(async () =>
        {
            var newHttpContext = new DefaultHttpContext();
            newHttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, adminId.ToString()),
                    new System.Security.Claims.Claim("permission", Yuviron.Domain.Enums.AppPermission.ManageCatalog.ToString())
                }, "BackgroundSync"));

            using var scope = scopeFactory.CreateScope();
            var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = newHttpContext;

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AdminJamendoController>>();

            try
            {
                logger.LogInformation("Background Jamendo sync started (Limit: {Limit}, Offset: {Offset}).", limit, offset);
                await mediator.Send(command, CancellationToken.None);
                logger.LogInformation("Background Jamendo sync completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during background Jamendo sync.");
            }
        });

        return Accepted(new SyncJamendoResponse("Синхронизация запущена в фоновом режиме. Результат можно отследить в логах сервера.", 0));
    }
}

public record SyncJamendoResponse(string Message, int SyncedTracksCount);