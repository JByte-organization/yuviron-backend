using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Moods.Commands.CreateMood;
using Yuviron.Application.Features.Admin.Moods.Commands.DeleteMood;
using Yuviron.Application.Features.Admin.Moods.Commands.UpdateMood;
using Yuviron.Application.Features.Admin.Moods.Queries.GetMoodById;
using Yuviron.Application.Features.Admin.Moods.Queries.GetMoods;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/moods")]
public class AdminMoodsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMoods([FromQuery] GetMoodsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMoodById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMoodByIdQuery(id), ct);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateMood([FromBody] CreateMoodCommand command, CancellationToken ct)
    {
        var moodId = await Mediator.Send(command, ct);
        return Ok(new { MoodId = moodId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMood(Guid id, [FromBody] UpdateMoodCommand command, CancellationToken ct)
    {
        var commandWithId = command with { Id = id };
        
        await Mediator.Send(commandWithId, ct);
        
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMood(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteMoodCommand(id), ct);
        return NoContent();
    }
}