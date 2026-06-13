using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.StudioArtist.Profile.Queries.GetStudioArtistProfile;
using Yuviron.Application.Features.StudioArtist.Artists.Commands.UpdateArtist;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.AddSocialLink;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.RemoveArtistPin;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.RemoveSocialLink;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.RequestVerification;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.SetArtistPin;
using Yuviron.Application.Features.StudioArtist.Profile.Commands.DeleteArtistAccount;
using Yuviron.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

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

    [HttpDelete("{artistId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteArtistAccount(Guid artistId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteArtistAccountCommand(artistId), ct);
        return NoContent();
    }
    
    [HttpPost("{artistId:guid}/social-links")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddSocialLink(Guid artistId, [FromBody] AddSocialLinkRequest request, CancellationToken ct)
    {
        await Mediator.Send(new AddSocialLinkCommand(artistId, request.Type, request.Url), ct);
        return NoContent();
    }

    [HttpDelete("{artistId:guid}/social-links/{type}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveSocialLink(Guid artistId, SocialLinkType type, CancellationToken ct)
    {
        await Mediator.Send(new RemoveSocialLinkCommand(artistId, type), ct);
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

public record AddSocialLinkRequest(SocialLinkType Type, string Url);
