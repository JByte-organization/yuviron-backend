using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Features.Auth.Commands.ChangePassword;
using Yuviron.Application.Features.Auth.Commands.Login;
using Yuviron.Application.Features.Auth.Commands.LoginWithCode;
using Yuviron.Application.Features.Auth.Commands.Logout;
using Yuviron.Application.Features.Auth.Commands.RefreshAccessToken;
using Yuviron.Application.Features.Auth.Commands.Register;
using Yuviron.Application.Features.Auth.Commands.SendLoginCode;
using Yuviron.Application.Features.Auth.Queries.CheckEmail;
using Yuviron.Domain.Enums;

namespace Yuviron.Api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    [HttpPost("check-email")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(CheckEmailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmail([FromBody] CheckEmailQuery query, CancellationToken ct)
    {
        var exists = await Mediator.Send(query, ct);
        return Ok(new CheckEmailResponse(exists));
    }

    public sealed record CheckEmailResponse(bool Exists);

    [HttpPost("send-code")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendCode([FromBody] SendLoginCodeCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return Ok();
    }

    [HttpPost("login-with-code")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginWithCode([FromBody] LoginWithCodeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        SetRefreshTokenCookie(result.RefreshToken);
        
        return Ok(new { result.UserId, result.Token, result.Email, result.Permissions });
    }

    [HttpPost("register")]
    [EnableRateLimiting("AuthPolicy")] 
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var userId = await Mediator.Send(command, ct);
        return Ok(new { UserId = userId });
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        SetRefreshTokenCookie(result.RefreshToken);
        
        return Ok(new { result.UserId, result.Token, result.Email, result.Permissions });
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
    public async Task<IActionResult> Refresh(
        [FromHeader(Name = "X-CSRF-Protection")] string? csrfHeader, 
        CancellationToken ct) 
    {
        if (string.IsNullOrEmpty(csrfHeader))
        {
            return Unauthorized("Missing Anti-CSRF header.");
        }

        var refreshToken = Request.Cookies["refreshToken"];
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Refresh token is missing.");
        }

        var command = new RefreshAccessTokenCommand(refreshToken);
        var result = await Mediator.Send(command, ct);
        
        SetRefreshTokenCookie(result.RefreshToken);
        
        return Ok(new { AccessToken = result.AccessToken });
    }
    
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await Mediator.Send(new LogoutCommand(refreshToken), ct);
            
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
        }
        
        return NoContent(); 
    }

    [HttpPost("change-password")]
    [Authorize] 
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(30) 
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}