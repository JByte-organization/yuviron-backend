using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.CreateTrack;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteLyrics;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateLyrics;
using Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.DTOs;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTrackById;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTrackLyrics;
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudioTrackDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudioTrackDetailsDto>> GetTrackById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetStudioTrackByIdQuery(id), ct);
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
    
    // --- LYRICS ---

    [HttpGet("{id:guid}/lyrics")]
    [ProducesResponseType(typeof(StudioTrackLyricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLyrics(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetStudioTrackLyricsQuery(id), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/lyrics")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateLyrics(Guid id, [FromBody] UpdateLyricsRequest request, CancellationToken ct)
    {
        await Mediator.Send(new UpdateLyricsCommand(id, request.LyricsText), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/lyrics")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteLyrics(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteLyricsCommand(id), ct);
        return NoContent();
    }
}

public record CreateTrackResponse(Guid TrackId);
public record UpdateLyricsRequest(string LyricsText);