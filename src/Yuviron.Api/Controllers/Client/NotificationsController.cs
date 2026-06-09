using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Features.Client.Notifications.Commands.UpdatePreferences;
using Yuviron.Application.Features.Client.Notifications.Commands.MarkAllAsRead;
using Yuviron.Application.Features.Client.Notifications.Commands.MarkAsRead;
using Yuviron.Application.Features.Client.Notifications.Queries.GetPreferences;
using Yuviron.Application.Features.Client.Notifications.Queries.GetNotifications;
using Yuviron.Application.Features.Client.Notifications.Queries.GetUnreadCount;

namespace Yuviron.Api.Controllers.Client;

[Authorize]
[Route("api/notifications")]
[ApiExplorerSettings(GroupName = "client")]
public class NotificationsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] GetNotificationsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var count = await Mediator.Send(new GetUnreadNotificationCountQuery(), ct);
        return Ok(count);
    }

    [HttpGet("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationPreferencesDto>> GetPreferences(CancellationToken ct)
        => Ok(await Mediator.Send(new GetNotificationPreferencesQuery(), ct));

    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferenceCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await Mediator.Send(new MarkAllNotificationsAsReadCommand(), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid id, CancellationToken ct)
    {
        await Mediator.Send(new MarkNotificationAsReadCommand(id), ct);
        return NoContent();
    }
}
