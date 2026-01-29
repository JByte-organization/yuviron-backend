using MediatR; 
using Microsoft.AspNetCore.Mvc; 
using Yuviron.Application.Features.Auth.Commands.Register;
using Yuviron.Application.Features.Auth.Commands.Login;

namespace Yuviron.Api.Controllers;

[Route("api/[controller]")] 
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
   
        var userId = await _mediator.Send(command);

        return Ok(new { UserId = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}