using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Auth.Commands.Register;
using Yuviron.Application.Features.Auth.Commands.Login;

namespace Yuviron.Api.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var userId = await Mediator.Send(command, ct);
        return Ok(new { UserId = userId });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)] // Swagger покаже структуру відповіді
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
}