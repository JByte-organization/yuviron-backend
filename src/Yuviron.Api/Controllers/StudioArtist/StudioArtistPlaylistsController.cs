using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.StudioArtist.Playlists.Commands.AddTrackToStudioPlaylist;
using Yuviron.Application.Features.StudioArtist.Playlists.Commands.ChangeTrackPositionInStudioPlaylist;
using Yuviron.Application.Features.StudioArtist.Playlists.Commands.CreatePlaylist;
using Yuviron.Application.Features.StudioArtist.Playlists.Commands.DeletePlaylist;
using Yuviron.Application.Features.StudioArtist.Playlists.Commands.RemoveTrackFromStudioPlaylist;
using Yuviron.Application.Features.StudioArtist.Playlists.Commands.UpdatePlaylist;
using Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylistById;
using Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylists;
using Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylistTracks;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/playlists")]
public class StudioArtistPlaylistsController : StudioArtistApiControllerBase
{
    [HttpGet("{artistId:guid}")]
    [ProducesResponseType(typeof(PaginatedList<StudioPlaylistListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(Guid artistId, [FromQuery] GetStudioPlaylistsQuery query, CancellationToken ct)
    {
        return Ok(await Mediator.Send(query with { ArtistId = artistId }, ct));
    }

    [HttpGet("details/{playlistId:guid}")]
    [ProducesResponseType(typeof(StudioPlaylistDetailsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid playlistId, CancellationToken ct)
    {
        return Ok(await Mediator.Send(new GetStudioPlaylistByIdQuery(playlistId), ct));
    }

    [HttpPost("{artistId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(Guid artistId, [FromBody] CreateStudioPlaylistCommand command, CancellationToken ct)
    {
        return Ok(await Mediator.Send(command with { ArtistId = artistId }, ct));
    }

    [HttpPut("{playlistId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid playlistId, [FromBody] UpdateStudioPlaylistCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { PlaylistId = playlistId }, ct);
        return NoContent();
    }

    [HttpDelete("{playlistId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid playlistId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteStudioPlaylistCommand(playlistId), ct);
        return NoContent();
    }

    [HttpGet("{playlistId:guid}/tracks")]
    [ProducesResponseType(typeof(PaginatedList<StudioPlaylistTrackItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTracks(Guid playlistId, [FromQuery] GetStudioPlaylistTracksQuery query, CancellationToken ct)
    {
        return Ok(await Mediator.Send(query with { PlaylistId = playlistId }, ct));
    }

    [HttpPost("{playlistId:guid}/tracks")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddTrack(Guid playlistId, [FromBody] AddTrackToStudioPlaylistCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { PlaylistId = playlistId }, ct);
        return NoContent();
    }

    [HttpDelete("{playlistId:guid}/tracks/{trackId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTrack(Guid playlistId, Guid trackId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTrackFromStudioPlaylistCommand(playlistId, trackId), ct);
        return NoContent();
    }

    [HttpPut("{playlistId:guid}/tracks/{trackId:guid}/position")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePosition(Guid playlistId, Guid trackId, [FromBody] ChangeTrackPositionInStudioPlaylistCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { PlaylistId = playlistId, TrackId = trackId }, ct);
        return NoContent();
    }
}