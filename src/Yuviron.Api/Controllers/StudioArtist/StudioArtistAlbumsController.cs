using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.StudioArtist.Albums.Commands.CreateAlbum;
using Yuviron.Application.Features.StudioArtist.Albums.Commands.DeleteAlbum;
using Yuviron.Application.Features.StudioArtist.Albums.Commands.UpdateAlbum;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.DTOs;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbumById;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbums;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbumTracks;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/albums")]
public class StudioArtistAlbumsController : StudioArtistApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<StudioAlbumListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<StudioAlbumListItemDto>>> GetAlbums([FromQuery] GetStudioAlbumsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudioAlbumDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudioAlbumDetailsDto>> GetAlbumById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetStudioAlbumByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/tracks")]
    [ProducesResponseType(typeof(List<StudioAlbumTrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<StudioAlbumTrackDto>>> GetAlbumTracks(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetStudioAlbumTracksQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateAlbumResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateAlbumResponse>> CreateAlbum([FromBody] CreateAlbumCommand command, CancellationToken ct)
    {
        var albumId = await Mediator.Send(command, ct);
        return Ok(new CreateAlbumResponse(albumId));
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAlbum(Guid id, [FromBody] UpdateAlbumCommand command, CancellationToken ct)
    {
        var commandWithId = command with { AlbumId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAlbum(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteAlbumCommand(id), ct);
        return NoContent();
    }
}

public record CreateAlbumResponse(Guid AlbumId);