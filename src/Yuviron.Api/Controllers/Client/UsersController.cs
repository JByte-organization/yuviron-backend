using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Library.Queries.GetUserFollowed;
using Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;
using Yuviron.Application.Features.Client.Users.Commands.FollowUser;
using Yuviron.Application.Features.Client.Users.Commands.UnfollowUser;
using Yuviron.Application.Features.Client.Users.Commands.UpdateUserProfile;
using Yuviron.Application.Features.Client.Users.Queries.GetUserFollowers;
using Yuviron.Application.Features.Client.Users.Queries.GetUserProfile;
using Yuviron.Application.Features.Client.Users.Queries.GetUserPublicPlaylists;

namespace Yuviron.Api.Controllers.Client;

[Route("api/users")] 
[ApiExplorerSettings(GroupName = "client")]
public class UsersController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet("{id:guid}/playlists")]
    [ProducesResponseType(typeof(PaginatedList<UserPlaylistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserPlaylistDto>>> GetUserPublicPlaylists(
        [FromRoute] Guid id, 
        [FromQuery] GetUserPublicPlaylistsQuery query, 
        CancellationToken ct)
    {
        var command = query with { TargetUserId = id }; 
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
    
    [HttpPost("{id:guid}/follow")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> FollowUser([FromRoute] Guid id, CancellationToken ct)
    {
        await Mediator.Send(new FollowUserCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/follow")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UnfollowUser([FromRoute] Guid id, CancellationToken ct)
    {
        await Mediator.Send(new UnfollowUserCommand(id), ct);
        return NoContent();
    }
    
    [HttpGet("{id:guid}/following")]
    [AllowAnonymous] 
    [ProducesResponseType(typeof(PaginatedList<FollowedProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<FollowedProfileDto>>> GetUserFollowing(
        [FromRoute] Guid id, 
        [FromQuery] GetFollowedProfilesQuery query, 
        CancellationToken ct)
    {
        var command = query with { TargetUserId = id }; 
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}/followers")]
    [AllowAnonymous] 
    [ProducesResponseType(typeof(PaginatedList<FollowerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<FollowerDto>>> GetUserFollowers(
        [FromRoute] Guid id, 
        [FromQuery] GetUserFollowersQuery query, 
        CancellationToken ct)
    {
        var command = query with { TargetUserId = id }; 
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetUserProfileQuery(id), ct);
        return Ok(result);
    }

    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request, CancellationToken ct)
    {
        var command = new UpdateUserProfileCommand(request.Name, request.Bio, request.AvatarFileId, request.BannerFileId);
        await Mediator.Send(command, ct);
        return NoContent();
    }
}