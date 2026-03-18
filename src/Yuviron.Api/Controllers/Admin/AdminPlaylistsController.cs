using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;
using Yuviron.Application.Features.Admin.Playlists.Commands.DeletePlaylist;
using Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;
using Yuviron.Application.Features.Admin.Playlists.Commands.AddTrackToPlaylist;
using Yuviron.Application.Features.Admin.Playlists.Commands.RemoveTrackFromPlaylist;
using Yuviron.Application.Features.Admin.Playlists.Commands.ChangeTrackPosition;
using Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistTracks;
using Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistById;
using Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylists;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/playlists")]
public class AdminPlaylistsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetPlaylistsQuery query, CancellationToken ct)
    {
        return Ok(await Mediator.Send(query, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await Mediator.Send(new GetPlaylistByIdQuery(id), ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistCommand command, CancellationToken ct)
    {
        var playlistId = await Mediator.Send(command, ct);
        return Ok(new { PlaylistId = playlistId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlaylistCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeletePlaylistCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/tracks")]
    public async Task<IActionResult> GetPlaylistTracks(Guid id, [FromQuery] GetPlaylistTracksQuery query, CancellationToken ct)
    {
        return Ok(await Mediator.Send(query with { PlaylistId = id }, ct));
    }

    [HttpPost("{id:guid}/tracks")]
    public async Task<IActionResult> AddTrack(Guid id, [FromBody] AddTrackToPlaylistCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { PlaylistId = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/tracks/{trackId:guid}")]
    public async Task<IActionResult> RemoveTrack(Guid id, Guid trackId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTrackFromPlaylistCommand(id, trackId), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/tracks/{trackId:guid}/position")]
    public async Task<IActionResult> ChangeTrackPosition(Guid id, Guid trackId, [FromBody] ChangeTrackPositionCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { PlaylistId = id, TrackId = trackId }, ct);
        return NoContent();
    }
}