using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Client.Genres.Queries.GetGenres;

namespace Yuviron.Api.Controllers.Client;

[Route("api/genres")]
[ApiExplorerSettings(GroupName = "client")]
[AllowAnonymous] 
public class GenresController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GenreItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGenres([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetGenresQuery(limit), ct);
        return Ok(result);
    }
}