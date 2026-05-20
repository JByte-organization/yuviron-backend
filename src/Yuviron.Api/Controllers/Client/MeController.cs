using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Library.Commands.AddTrackToFavorites;
using Yuviron.Application.Features.Client.Library.Commands.RemoveTrackFromFavorites;
using Yuviron.Application.Features.Client.Library.Queries.GetFollowedArtists;
using Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteTracks;
using Yuviron.Application.Features.Client.Library.Queries.GetUserFollowed;
using Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;
using Yuviron.Application.Features.Client.Users.Queries.GetUserFollowers;

namespace Yuviron.Api.Controllers.Client;

[Route("api/me")] 
[ApiExplorerSettings(GroupName = "client")]
[Authorize] 
public class MeController : ApiControllerBase
{
    [HttpGet("favorites/tracks")]
    [ProducesResponseType(typeof(PaginatedList<UserFavoriteTrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserFavoriteTrackDto>>> GetFavoriteTracks([FromQuery] GetUserFavoriteTracksQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
    [HttpPost("favorites/tracks")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddTrackToFavorites([FromBody] AddTrackToFavoritesCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }
    
    [HttpDelete("favorites/tracks/{trackId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTrackFromFavorites(Guid trackId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTrackFromFavoritesCommand(trackId), ct);
        return NoContent();
    }

    [HttpGet("following/artists")]
    [ProducesResponseType(typeof(PaginatedList<FollowedArtistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FollowedArtistDto>>> GetFavoriteArtists([FromQuery] GetFollowedArtistsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("recently-played")]
    [ProducesResponseType(typeof(List<RecentlyPlayedTrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RecentlyPlayedTrackDto>>> GetRecentlyPlayed([FromQuery] GetUserRecentlyPlayedQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
    [HttpGet("following")]
    [ProducesResponseType(typeof(PaginatedList<FollowedProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FollowedProfileDto>>> GetMyFollowing(
        [FromQuery] GetFollowedProfilesQuery query, 
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
    [HttpGet("followers")]
    [ProducesResponseType(typeof(PaginatedList<FollowerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FollowerDto>>> GetMyFollowers(
        [FromQuery] GetUserFollowersQuery query, 
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
}