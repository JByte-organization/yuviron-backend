using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Client.Library.Commands.AddTrackToFavorites;
using Yuviron.Application.Features.Client.Library.Commands.RemoveTrackFromFavorites;
using Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteTracks;
using Yuviron.Application.Common;

namespace Yuviron.Api.Controllers.Client;

[Route("api/user/favorites")]
[ApiExplorerSettings(GroupName = "client")]
[Authorize]
public class LibraryController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddTrackToFavorites([FromBody] AddTrackToFavoritesCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<UserFavoriteTrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<UserFavoriteTrackDto>>> GetFavoriteTracks([FromQuery] GetUserFavoriteTracksQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpDelete("{trackId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTrackFromFavorites(Guid trackId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTrackFromFavoritesCommand(trackId), ct);
        return NoContent();
    }
}
