using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.StudioArtist.Profile.Queries.GetStudioArtistProfile;
using Yuviron.Application.Features.StudioArtist.Artists.Commands.UpdateArtist;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.RemoveArtistPin;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.RequestVerification;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.SetArtistPin;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.UpdateSocialLinks;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/profile")]
public class StudioArtistProfileController : StudioArtistApiControllerBase
{
    [HttpGet("{artistId:guid}")]
    [ProducesResponseType(typeof(StudioArtistProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudioArtistProfileDto>> GetProfile(Guid artistId, CancellationToken ct)
    {
        return Ok(await Mediator.Send(new GetStudioArtistProfileQuery(artistId), ct));
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateArtist(Guid id, [FromBody] UpdateArtistCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { ArtistId = id }, ct);
        return NoContent();
    }
    
    [HttpPut("{artistId:guid}/social-links")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateSocialLinks(Guid artistId, [FromBody] List<SocialLinkItem> links, CancellationToken ct)
    {
        await Mediator.Send(new UpdateSocialLinksCommand(artistId, links), ct);
        return NoContent();
    }

    [HttpPost("{artistId:guid}/pins")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetPin(Guid artistId, [FromBody] SetArtistPinCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { ArtistId = artistId }, ct);
        return NoContent();
    }

    [HttpDelete("{artistId:guid}/pins/{position:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemovePin(Guid artistId, int position, CancellationToken ct)
    {
        await Mediator.Send(new RemoveArtistPinCommand(artistId, position), ct);
        return NoContent();
    }
    
    [HttpPost("{artistId:guid}/verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RequestVerification(Guid artistId, [FromBody] RequestArtistVerificationCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { ArtistId = artistId }, ct);
        return NoContent();
    }
}