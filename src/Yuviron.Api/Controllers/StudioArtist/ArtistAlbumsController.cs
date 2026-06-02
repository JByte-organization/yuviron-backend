using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.GetAlbums;

namespace Yuviron.Api.Controllers.StudioArtist;

[Authorize]
[Route("api/studio-artist/albums")]
[ApiExplorerSettings(GroupName = "artist")]
public class ArtistAlbumsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AlbumListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<AlbumListItemDto>>> GetAlbums(
        [FromQuery] GetStudioArtistAlbumsQuery query,
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
}
