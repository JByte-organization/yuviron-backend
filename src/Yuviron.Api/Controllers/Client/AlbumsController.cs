using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Albums.Queries.GetAlbumById;
using Yuviron.Application.Features.Client.Albums.Queries.GetAlbumTracks;

namespace Yuviron.Api.Controllers.Client;

[Route("api/albums")]
[ApiExplorerSettings(GroupName = "client")]
public class AlbumsController : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AlbumDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlbumDetailsDto>> GetAlbumById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetAlbumByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/tracks")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AlbumTrackItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AlbumTrackItemDto>>> GetAlbumTracks([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetAlbumTracksQuery(id), ct);
        return Ok(result);
    }
}
