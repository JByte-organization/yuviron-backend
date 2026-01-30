using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Users.GetUserProfile;


namespace Yuviron.Api.Controllers;

public class UsersController : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserProfile(Guid id)
    {
        var query = new GetUserProfileQuery(id);
        var result = await Mediator.Send(query);

        return Ok(result);
    }
}