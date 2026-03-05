using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;
using Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrack;
using Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrack;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetTrackById;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetTracks;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/tracks")]
public class AdminTracksController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTracks([FromQuery] GetTracksQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTrackById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTrackByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTrack([FromBody] CreateTrackCommand command, CancellationToken ct)
    {
        var trackId = await Mediator.Send(command, ct);
        return Ok(new { TrackId = trackId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTrack(Guid id, [FromBody] UpdateTrackCommand command, CancellationToken ct)
    {
        var commandWithId = command with { TrackId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTrack(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteTrackCommand(id), ct);
        return NoContent();
    }
}