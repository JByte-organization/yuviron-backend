using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Moods.Commands.CreateMood;
using Yuviron.Application.Features.Admin.Moods.Commands.DeleteMood;
using Yuviron.Application.Features.Admin.Moods.Commands.UpdateMood;
using Yuviron.Application.Features.Admin.Moods.Queries.DTOs;
using Yuviron.Application.Features.Admin.Moods.Queries.GetMoodById;
using Yuviron.Application.Features.Admin.Moods.Queries.GetMoods;
using Yuviron.Application.Features.Admin.Moods.Queries.GetMoodsAutocomplete;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/moods")]
public class AdminMoodsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<MoodDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<MoodDto>>> GetMoods([FromQuery] GetMoodsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetMoodByIdDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetMoodByIdDto>> GetMoodById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMoodByIdQuery(id), ct);
        return Ok(result);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(CreateMoodResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateMoodResponse>> CreateMood([FromBody] CreateMoodCommand command, CancellationToken ct)
    {
        var moodId = await Mediator.Send(command, ct);
        return Ok(new CreateMoodResponse(moodId));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMood(Guid id, [FromBody] UpdateMoodCommand command, CancellationToken ct)
    {
        var commandWithId = command with { Id = id };
        
        await Mediator.Send(commandWithId, ct);
        
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMood(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteMoodCommand(id), ct);
        return NoContent();
    }
    
    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(List<MoodAutocompleteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MoodAutocompleteDto>>> AutocompleteMoods([FromQuery] string searchTerm, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetMoodsAutocompleteQuery(searchTerm, limit), ct);
        return Ok(result);
    }
}
