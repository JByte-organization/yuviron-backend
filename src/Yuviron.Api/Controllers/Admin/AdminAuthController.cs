using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Yuviron.Application.Features.Admin.Auth.Commands.AdminPreLogin;
using Yuviron.Application.Features.Admin.Auth.Commands.AdminLogin;
using Yuviron.Application.Features.Auth.Commands.Login;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/auth")]
[AllowAnonymous] 
public class AdminAuthController : ApiControllerBase
{
    [HttpPost("pre-login")]
    [EnableRateLimiting("AuthPolicy")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PreLogin([FromBody] AdminPreLoginCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return Ok();
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] AdminLoginCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddHours(12) 
        };

        Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
        
        return Ok(new { result.UserId, result.Token, result.Email, result.Permissions });
    }
}