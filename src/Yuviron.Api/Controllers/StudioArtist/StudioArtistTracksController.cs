using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.DTOs;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTracks;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/tracks")]
public class StudioArtistTracksController : StudioArtistApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<StudioTrackListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<StudioTrackListItemDto>>> GetTracks([FromQuery] GetStudioTracksQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
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
}

public record CreateTrackResponse(Guid TrackId);