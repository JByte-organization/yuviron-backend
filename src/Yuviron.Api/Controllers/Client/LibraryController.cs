using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Library.Commands.AddTrackToFavorites;
using Yuviron.Application.Features.Client.Library.Commands.RemoveTrackFromFavorites;
using Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteArtists;
using Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteTracks;
using Yuviron.Application.Features.Client.Library.Queries.GetUserPlaylists;
using Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;

namespace Yuviron.Api.Controllers.Client;

[Route("api/user")]
[ApiExplorerSettings(GroupName = "client")]
[Authorize]
public class LibraryController : ApiControllerBase
{
    
    [HttpGet("favorites")]
    [ProducesResponseType(typeof(PaginatedList<UserFavoriteTrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserFavoriteTrackDto>>> GetFavoriteTracks([FromQuery] GetUserFavoriteTracksQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
    [HttpPost("favorites")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddTrackToFavorites([FromBody] AddTrackToFavoritesCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }
    
    [HttpDelete("favorites/{trackId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTrackFromFavorites(Guid trackId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTrackFromFavoritesCommand(trackId), ct);
        return NoContent();
    }

    [HttpGet("followed-artists")]
    [ProducesResponseType(typeof(PaginatedList<FollowedArtistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FollowedArtistDto>>> GetFavoriteArtists([FromQuery] GetFollowedArtistsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
}
