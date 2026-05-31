using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.ClaimProfile;
using Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.CreateProfile;

namespace Yuviron.Api.Controllers.Client; 

[Authorize] 
[Route("api/artist-profiles")] 
[ApiExplorerSettings(GroupName = "client")] 
public class ArtistProfilesController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateArtistProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
    [ProducesResponseType(StatusCodes.Status403Forbidden)] 
    public async Task<IActionResult> CreateProfile([FromBody] CreateArtistProfileRequest request, CancellationToken ct)
    {
        var command = new CreateArtistProfileCommand(request.Name, request.AvatarFileId);
        
        var artistId = await Mediator.Send(command, ct);
        
        return Ok(new CreateArtistProfileResponse(artistId));
    }

    [HttpPost("{id:guid}/claim")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
    [ProducesResponseType(StatusCodes.Status403Forbidden)] 
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClaimProfile([FromRoute] Guid id, [FromBody] ClaimArtistProfileRequest request, CancellationToken ct)
    {
        var command = new ClaimArtistProfileCommand(
            ArtistId: id,
            ClaimedRole: request.ClaimedRole,
            OfficialEmail: request.OfficialEmail,
            Links: request.Links,
            ProofFileId: request.ProofFileId,
            Message: request.Message
        );
        
        await Mediator.Send(command, ct);
        
        return NoContent(); 
    }
}