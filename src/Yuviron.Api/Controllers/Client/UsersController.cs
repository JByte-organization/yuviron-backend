using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Users.Queries.GetUserPublicPlaylists;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;

namespace Yuviron.Api.Controllers.Client;

[Route("api/users")]
[ApiExplorerSettings(GroupName = "client")]
public class UsersController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet("{id:guid}/playlists")]
    [ProducesResponseType(typeof(PaginatedList<UserPlaylistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserPlaylistDto>>> GetUserPublicPlaylists(
        [FromRoute] Guid id, 
        [FromQuery] GetUserPublicPlaylistsQuery query, 
        CancellationToken ct)
    {
        var command = query with { TargetUserId = id }; 
        var result = await Mediator.Send(command, ct);
        
        return Ok(result);
    }
}