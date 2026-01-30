using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Playlists.Commands.CreatePlaylist;
using Yuviron.Application.Features.Playlists.Queries.GetPlaylistById;

namespace Yuviron.Api.Controllers;

public class PlaylistsController : ApiControllerBase
{

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistCommand command)
    {
        var secureCommand = command with { UserId = UserId };

        var playlistId = await Mediator.Send(secureCommand);

        return Ok(playlistId);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id)
    {


        Guid? currentUserId = UserId == Guid.Empty ? null : UserId;

        var query = new GetPlaylistQuery(id, currentUserId);
        var result = await Mediator.Send(query);

        return Ok(result);
    }
}