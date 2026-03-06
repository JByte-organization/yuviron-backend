using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Moods.Commands.CreateMood;
using Yuviron.Application.Features.Admin.Moods.Commands.DeleteMood;
using Yuviron.Application.Features.Admin.Moods.Commands.UpdateMood;
using Yuviron.Application.Features.Admin.Moods.Queries.GetMoods;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/moods")]
public class AdminMoodsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<MoodDto>>> Get([FromQuery] GetMoodsQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateMoodCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateMoodCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Path ID and Body ID mismatch.");
        }

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteMoodCommand(id));
        return NoContent();
    }
}