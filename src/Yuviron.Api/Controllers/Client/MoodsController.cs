using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Moods.Queries.GetMoods;

namespace Yuviron.Api.Controllers.Client;

[Route("api/moods")]
[ApiExplorerSettings(GroupName = "client")]
[AllowAnonymous]
public class MoodsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<MoodItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMoods([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetMoodsQuery(limit), ct);
        return Ok(result);
    }
}