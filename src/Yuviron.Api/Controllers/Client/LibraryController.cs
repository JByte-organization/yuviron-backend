using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Library.Commands.AddTrackToFavorites;
using Yuviron.Application.Features.Client.Library.Commands.RemoveTrackFromFavorites;
using Yuviron.Application.Features.Client.Library.Commands.TrackPlay;
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
    [HttpPost("favorites")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddTrackToFavorites([FromBody] AddTrackToFavoritesCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpGet("favorite-tracks")]
    [ProducesResponseType(typeof(PaginatedList<UserFavoriteTrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserFavoriteTrackDto>>> GetFavoriteTracks([FromQuery] GetUserFavoriteTracksQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("playlists")]
    [ProducesResponseType(typeof(PaginatedList<UserPlaylistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserPlaylistDto>>> GetUserPlaylists([FromQuery] GetUserPlaylistsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("favorite-artists")]
    [ProducesResponseType(typeof(PaginatedList<UserFavoriteArtistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserFavoriteArtistDto>>> GetFavoriteArtists([FromQuery] GetUserFavoriteArtistsQuery query, CancellationToken ct)
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

    [HttpPost("track-play")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TrackPlay([FromBody] TrackPlayRequest request, CancellationToken ct)
    {
        var countryCodeHeader = Request.Headers["CF-IPCountry"].FirstOrDefault();
        var trimmedCountryCode = string.IsNullOrWhiteSpace(countryCodeHeader)
            ? null
            : countryCodeHeader.Trim();
        var countryCode = trimmedCountryCode is { Length: 2 } ? trimmedCountryCode : null;

        await Mediator.Send(new TrackPlayCommand(request.TrackId, countryCode), ct);
        return Ok();
    }

    [HttpDelete("favorites/{trackId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTrackFromFavorites(Guid trackId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTrackFromFavoritesCommand(trackId), ct);
        return NoContent();
    }
}
