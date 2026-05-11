using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistById; // <-- Добавили using
using Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistTracks;

namespace Yuviron.Api.Controllers.Client;

[Route("api/playlists")]
[ApiExplorerSettings(GroupName = "client")]
public class PlaylistsController : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlaylistDetailsClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlaylistById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetPlaylistByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/tracks")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedList<PlaylistTrackItemClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<PlaylistTrackItemClientDto>>> GetTracks([FromRoute] Guid id, [FromQuery] GetPlaylistTracksQuery query, CancellationToken ct = default)
    {
        var command = query with { PlaylistId = id };
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
}