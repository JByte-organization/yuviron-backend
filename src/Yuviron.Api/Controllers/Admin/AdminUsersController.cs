using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Users.Commands.BlockUser;
using Yuviron.Application.Features.Admin.Users.Commands.CreateUser;
using Yuviron.Application.Features.Admin.Users.Commands.DeleteUser;
using Yuviron.Application.Features.Admin.Users.Commands.UnblockUser;
using Yuviron.Application.Features.Admin.Users.Commands.UpdateUser;
using Yuviron.Application.Features.Admin.Users.Queries.GetUserById;
using Yuviron.Application.Features.Admin.Users.Queries.GetUsers;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/users")]
public class AdminUsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var userId = await Mediator.Send(command, ct);
        return Ok(new { UserId = userId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        var commandWithId = command with { UserId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteUserCommand(id), ct);
        return NoContent();
    }
    
    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> BlockUser(Guid id, [FromBody] BlockUserCommand command, CancellationToken ct)
    {
        var commandWithId = command with { UserId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new UnblockUserCommand(id), ct);
        return NoContent();
    }
}