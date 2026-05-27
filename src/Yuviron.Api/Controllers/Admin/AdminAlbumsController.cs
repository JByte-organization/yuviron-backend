using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Commands.CreateAlbum;
using Yuviron.Application.Features.Admin.Albums.Commands.DeleteAlbum;
using Yuviron.Application.Features.Admin.Albums.Commands.UpdateAlbum;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;
using Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumById;
using Yuviron.Application.Features.Admin.Albums.Queries.GetAlbums;
using Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumsAutocomplete;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/albums")]
public class AdminAlbumsController : AdminApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AlbumListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<AlbumListItemDto>>> GetAlbums([FromQuery] GetAlbumsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AlbumDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AlbumDetailsDto>> GetAlbumById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAlbumByIdQuery(id), ct);
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
    public async Task<IActionResult> UpdateAlbum(Guid id, [FromBody] UpdateAlbumCommand command, CancellationToken ct)
    {
        var commandWithId = command with { AlbumId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAlbum(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteAlbumCommand(id), ct);
        return NoContent();
    }
    
    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(List<AlbumAutocompleteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlbumAutocompleteDto>>> Autocomplete([FromQuery] string searchTerm, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetAlbumsAutocompleteQuery(searchTerm, limit), ct);
        return Ok(result);
    }
}
