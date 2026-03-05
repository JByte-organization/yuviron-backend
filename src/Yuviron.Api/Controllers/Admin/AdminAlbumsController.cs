using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Albums.Commands.CreateAlbum;
using Yuviron.Application.Features.Admin.Albums.Commands.DeleteAlbum;
using Yuviron.Application.Features.Admin.Albums.Commands.UpdateAlbum;
using Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumById;
using Yuviron.Application.Features.Admin.Albums.Queries.GetAlbums;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/albums")]
public class AdminAlbumsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAlbums([FromQuery] GetAlbumsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAlbumById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAlbumByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAlbum([FromBody] CreateAlbumCommand command, CancellationToken ct)
    {
        var albumId = await Mediator.Send(command, ct);
        return Ok(new { AlbumId = albumId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAlbum(Guid id, [FromBody] UpdateAlbumCommand command, CancellationToken ct)
    {
        var commandWithId = command with { AlbumId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAlbum(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteAlbumCommand(id), ct);
        return NoContent();
    }
}