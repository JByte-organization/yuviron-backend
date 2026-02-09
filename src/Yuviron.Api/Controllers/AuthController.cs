using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Features.Auth.Commands.Login;
using Yuviron.Application.Features.Auth.Commands.LoginWithCode;
using Yuviron.Application.Features.Auth.Commands.RefreshAccessToken;
using Yuviron.Application.Features.Auth.Commands.Register;
using Yuviron.Application.Features.Auth.Commands.SendLoginCode;
using Yuviron.Application.Features.Auth.Queries.CheckEmail;
using Yuviron.Domain.Enums;

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
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("me/permissions")]
    [Authorize]
    [ProducesResponseType(typeof(List<AppPermission>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPermissions(
        [FromServices] IUserContext userContext,
        [FromServices] IPermissionService permissionService)
    {
        var permissionStrings = await permissionService.GetPermissionsAsync(userContext.UserId);

        var result = new List<AppPermission>();

        foreach (var permStr in permissionStrings)
        {
            if (Enum.TryParse<AppPermission>(permStr, out var parsedEnum))
            {
                result.Add(parsedEnum);
            }
        }

        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshAccessTokenCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}