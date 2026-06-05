using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;
using Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrack;
using Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrackLyrics;
using Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrack;
using Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrackLyrics;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetTrackById;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetTrackLyrics;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetTracks;
using Yuviron.Application.Features.Admin.Tracks.Queries.GetTracksAutocomplete;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/tracks")]
public class AdminTracksController : AdminApiControllerBase
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

    // --- LYRICS ---
    
    [HttpGet("{id:guid}/lyrics")]
    [ProducesResponseType(typeof(AdminTrackLyricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminTrackLyricsDto>> GetTrackLyrics(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTrackLyricsQuery(id), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/lyrics")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateTrackLyrics(Guid id, [FromBody] UpdateTrackLyricsRequest request, CancellationToken ct)
    {
        await Mediator.Send(new UpdateTrackLyricsCommand(id, request.LyricsText), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/lyrics")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTrackLyrics(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteTrackLyricsCommand(id), ct);
        return NoContent();
    }
}

public record UpdateTrackLyricsRequest(string LyricsText);