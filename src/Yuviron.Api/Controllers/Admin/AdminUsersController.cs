using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Users.Commands.BlockUser;
using Yuviron.Application.Features.Admin.Users.Commands.CreateUser;
using Yuviron.Application.Features.Admin.Users.Commands.DeleteUser;
using Yuviron.Application.Features.Admin.Users.Commands.UnblockUser;
using Yuviron.Application.Features.Admin.Users.Commands.UpdateUser;
using Yuviron.Application.Features.Admin.Users.Queries.DTOs;
using Yuviron.Application.Features.Admin.Users.Queries.GetUserById;
using Yuviron.Application.Features.Admin.Users.Queries.GetUsers;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/users")]
public class AdminUsersController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<UserListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserListItemDto>>> GetUsers([FromQuery] GetUsersQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDetailsDto>> GetUserById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateUserResponse>> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var userId = await Mediator.Send(command, ct);
        return Ok(new CreateUserResponse(userId));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        var commandWithId = command with { UserId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteUserCommand(id), ct);
        return NoContent();
    }
    
    [HttpPost("{id:guid}/block")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BlockUser(Guid id, [FromBody] BlockUserCommand command, CancellationToken ct)
    {
        var commandWithId = command with { UserId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/unblock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UnblockUser(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new UnblockUserCommand(id), ct);
        return NoContent();
    }
}
