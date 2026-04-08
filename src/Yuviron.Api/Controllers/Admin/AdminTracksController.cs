using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;
using Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrack;
using Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrack;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetTrackById;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetTracks;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetTracksAutocomplete;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/tracks")]
public class AdminTracksController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<TrackListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<TrackListItemDto>>> GetTracks([FromQuery] GetTracksQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TrackDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TrackDetailsDto>> GetTrackById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTrackByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateTrackResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateTrackResponse>> CreateTrack([FromBody] CreateTrackCommand command, CancellationToken ct)
    {
        var trackId = await Mediator.Send(command, ct);
        return Ok(new CreateTrackResponse(trackId));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateTrack(Guid id, [FromBody] UpdateTrackCommand command, CancellationToken ct)
    {
        var commandWithId = command with { TrackId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTrack(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteTrackCommand(id), ct);
        return NoContent();
    }
    
    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(List<TrackAutocompleteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TrackAutocompleteDto>>> Autocomplete([FromQuery] string searchTerm, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetTracksAutocompleteQuery(searchTerm, limit), ct);
        return Ok(result);
    }
}
