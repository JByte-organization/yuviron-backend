using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistTopTracks;

namespace Yuviron.Api.Controllers.Client;

[Route("api/artists")]
[ApiExplorerSettings(GroupName = "client")]
public class ArtistsController : ApiControllerBase
{
    [HttpGet("{id:guid}/top-tracks")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ArtistTopTrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArtistTopTracks([FromRoute] Guid id, [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetArtistTopTracksQuery(id, limit), ct);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}/albums")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedList<ArtistAlbumDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<ArtistAlbumDto>>> GetArtistAlbums([FromRoute] Guid id, [FromQuery] GetArtistAlbumsQuery query, CancellationToken ct = default)
    {
        var queryWithId = query with { ArtistId = id };
        var result = await Mediator.Send(queryWithId, ct);
        return Ok(result);
    }
}