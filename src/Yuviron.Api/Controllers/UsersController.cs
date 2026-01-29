using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Users.Queries.GetUserProfile;

namespace Yuviron.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetUserProfile(Guid id)
    {
        var query = new GetUserProfileQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}