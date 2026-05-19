using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;
using Yuviron.Application.Features.Client.Search.Queries.SearchAlbums;
using Yuviron.Application.Features.Client.Search.Queries.SearchArtists;
using Yuviron.Application.Features.Client.Search.Queries.SearchGenres;
using Yuviron.Application.Features.Client.Search.Queries.SearchPlaylists;
using Yuviron.Application.Features.Client.Search.Queries.SearchTracks;

namespace Yuviron.Api.Controllers.Client;

[Route("api/search")]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "client")]
public class SearchController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GlobalSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GlobalSearch([FromQuery] string? query, [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GlobalSearchQuery(query, limit), ct);
        return Ok(result);
    }

    [HttpGet("tracks")]
    [ProducesResponseType(typeof(PaginatedList<SearchTrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchTracks(
        [FromQuery] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new SearchTracksQuery(query, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("artists")]
    [ProducesResponseType(typeof(PaginatedList<SearchArtistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchArtists(
        [FromQuery] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new SearchArtistsQuery(query, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("albums")]
    [ProducesResponseType(typeof(PaginatedList<SearchAlbumDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchAlbums(
        [FromQuery] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new SearchAlbumsQuery(query, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("playlists")]
    [ProducesResponseType(typeof(PaginatedList<SearchPlaylistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchPlaylists(
        [FromQuery] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new SearchPlaylistsQuery(query, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("genres")]
    [ProducesResponseType(typeof(PaginatedList<SearchGenreMoodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchGenres(
        [FromQuery] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new SearchGenresQuery(query, page, pageSize), ct);
        return Ok(result);
    }
}
