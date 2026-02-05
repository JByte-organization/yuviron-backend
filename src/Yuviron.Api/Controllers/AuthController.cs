using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Auth.Commands.Login;
using Yuviron.Application.Features.Auth.Commands.LoginWithCode;
using Yuviron.Application.Features.Auth.Commands.Register;
using Yuviron.Application.Features.Auth.Commands.SendLoginCode;
using Yuviron.Application.Features.Auth.Queries.CheckEmail;

namespace Yuviron.Api.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("check-email")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmail([FromBody] CheckEmailQuery query, CancellationToken ct)
    {
        var exists = await Mediator.Send(query, ct);
        return Ok(new { Exists = exists });
    }


    [HttpPost("send-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendCode([FromBody] SendLoginCodeCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return Ok();
    }

    [HttpPost("login-with-code")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginWithCode([FromBody] LoginWithCodeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }

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