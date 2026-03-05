using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;
using Yuviron.Application.Features.Admin.Artists.Commands.DeleteArtist;
using Yuviron.Application.Features.Admin.Artists.Commands.UpdateArtist;
using Yuviron.Application.Features.Admin.Artists.Queries.GetArtistById;
using Yuviron.Application.Features.Admin.Artists.Queries.GetArtists;

namespace Yuviron.Api.Controllers.Admin;

[Authorize]
[Route("api/admin/artists")]
public class AdminArtistsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetArtists([FromQuery] GetArtistsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetArtistById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetArtistByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateArtist([FromBody] CreateArtistCommand command, CancellationToken ct)
    {
        var artistId = await Mediator.Send(command, ct);
        return Ok(new { ArtistId = artistId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateArtist(Guid id, [FromBody] UpdateArtistCommand command, CancellationToken ct)
    {
        var commandWithId = command with { ArtistId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteArtist(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteArtistCommand(id), ct);
        return NoContent();
    }
}