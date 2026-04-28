using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

namespace Yuviron.Api.Controllers;

[Route("api/search")]
[AllowAnonymous]
public class SearchController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GlobalSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GlobalSearch([FromQuery] string query, [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GlobalSearchQuery(query, limit), ct);
        return Ok(result);
    }
}