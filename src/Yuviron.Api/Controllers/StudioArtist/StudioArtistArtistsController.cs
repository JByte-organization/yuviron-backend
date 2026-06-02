using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.StudioArtist.Artists.Commands.UpdateArtist;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/artists")]
public class StudioArtistArtistsController : StudioArtistApiControllerBase
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateArtist(Guid id, [FromBody] UpdateArtistCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { ArtistId = id }, ct);
        return NoContent();
    }
}