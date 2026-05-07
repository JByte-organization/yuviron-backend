using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;
using Yuviron.Application.Features.Client.Playlists.Commands.UpdatePlaylist;
using Yuviron.Application.Features.Client.Playlists.Commands.DeletePlaylist;
using Yuviron.Application.Features.Client.Playlists.Commands.AddTrackToPlaylist;
using Yuviron.Application.Features.Client.Playlists.Commands.RemoveTrackFromPlaylist;

namespace Yuviron.Api.Controllers.Client;

[Authorize]
[Route("api/user/playlists")]
[ApiExplorerSettings(GroupName = "client")]
public class UserPlaylistsController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreatePlaylistResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistRequest request, CancellationToken ct)
    {
        var command = new CreatePlaylistCommand(request.Name, request.CoverUrl);
        var result = await Mediator.Send(command, ct);
        
        return Ok(new CreatePlaylistResponse(result));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePlaylist([FromRoute] Guid id, [FromBody] UpdatePlaylistRequest request, CancellationToken ct)
    {
        var command = new UpdatePlaylistCommand(id, request.Name, request.CoverUrl);
        await Mediator.Send(command, ct);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePlaylist([FromRoute] Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeletePlaylistCommand(id), ct);
        
        return Ok();
    }

    [HttpPost("{id:guid}/tracks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddTrack([FromRoute] Guid id, [FromBody] AddTrackRequest request, CancellationToken ct)
    {
        await Mediator.Send(new AddTrackToPlaylistCommand(id, request.TrackId), ct);
        
        return Ok();
    }

    [HttpDelete("{id:guid}/tracks/{trackId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveTrack([FromRoute] Guid id, [FromRoute] Guid trackId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTrackFromPlaylistCommand(id, trackId), ct);
        
        return Ok();
    }
}