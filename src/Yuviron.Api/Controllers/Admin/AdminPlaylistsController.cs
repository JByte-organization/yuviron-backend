using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;
using Yuviron.Application.Features.Admin.Playlists.Commands.DeletePlaylist;
using Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;
using Yuviron.Application.Features.Admin.Playlists.Queries;
using Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistById;
using Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylists;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/playlists")]
public class AdminPlaylistsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<PlaylistDto>>> GetAll([FromQuery] GetPlaylistsQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlaylistDetailsDto>> GetById(Guid id)
    {
        return Ok(await Mediator.Send(new GetPlaylistByIdQuery(id)));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePlaylistCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePlaylistCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Path ID and Body ID mismatch.");
        }

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeletePlaylistCommand(id));
        return NoContent();
    }
}