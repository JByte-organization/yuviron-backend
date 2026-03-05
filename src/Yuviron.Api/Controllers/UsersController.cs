using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Users.Queries.GetUserProfile;

namespace Yuviron.Api.Controllers;

public class UsersController : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserProfile(Guid id, CancellationToken ct)
    {
        var query = new GetUserProfileQuery(id);
        var result = await Mediator.Send(query, ct);

        return Ok(result);
    }
}
