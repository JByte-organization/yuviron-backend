using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.CreateProfile;

namespace Yuviron.Api.Controllers.ArtistDashboard;

[Route("api/artist-dashboard/profiles")]
[Authorize] 
public class ArtistProfilesController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateArtistProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
    [ProducesResponseType(StatusCodes.Status403Forbidden)] 
    public async Task<IActionResult> CreateProfile([FromBody] CreateArtistProfileRequest request, CancellationToken ct)
    {
        var command = new CreateArtistProfileCommand(request.Name, request.AvatarUrl);
        
        var artistId = await Mediator.Send(command, ct);
        
        return Ok(new CreateArtistProfileResponse(artistId));
    }
}