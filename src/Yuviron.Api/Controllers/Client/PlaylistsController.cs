using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Library.Queries.GetUserPlaylists;
using Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;
using Yuviron.Application.Features.Client.Playlists.Commands.UpdatePlaylist;
using Yuviron.Application.Features.Client.Playlists.Commands.DeletePlaylist;
using Yuviron.Application.Features.Client.Playlists.Commands.AddTrackToPlaylist;
using Yuviron.Application.Features.Client.Playlists.Commands.RemoveTrackFromPlaylist;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;
using Yuviron.Application.Features.Client.Playlists.Commands.ChangeTrackPosition;
using Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistTracks;

namespace Yuviron.Api.Controllers.Client;

[Authorize]
[Route("api/user/playlists")]
[ApiExplorerSettings(GroupName = "client")]
public class PlaylistsController : ApiControllerBase
{
    
    [HttpGet("playlists")]
    [ProducesResponseType(typeof(PaginatedList<UserPlaylistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserPlaylistDto>>> GetUserPlaylists([FromQuery] GetUserPlaylistsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
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
    
    [AllowAnonymous]
    [HttpGet("{id:guid}/tracks")]
    [ProducesResponseType(typeof(PaginatedList<PlaylistTrackItemClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<PlaylistTrackItemClientDto>>> GetTracks([FromRoute] Guid id, [FromQuery] GetPlaylistTracksQuery query, CancellationToken ct)
    {
        var command = query with { PlaylistId = id };
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/tracks/{trackId:guid}/position")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeTrackPosition([FromRoute] Guid id, [FromRoute] Guid trackId, [FromBody] ChangeTrackPositionRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ChangeTrackPositionCommand(id, trackId, request.NewPosition), ct);
        return Ok();
    }
}
