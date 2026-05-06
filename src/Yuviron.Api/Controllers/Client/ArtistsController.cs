using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistById;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistPlaylists;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistPopularReleases;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistRelatedTracks;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistSimilarArtists;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistSingles;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistTopTracks;

namespace Yuviron.Api.Controllers.Client;

[Route("api/artists")]
[ApiExplorerSettings(GroupName = "client")]
public class ArtistsController : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ArtistDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArtistById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetArtistByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/top-tracks")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ArtistTopTrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArtistTopTracks([FromRoute] Guid id, [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetArtistTopTracksQuery(id, limit), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/popular-releases")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ArtistAlbumDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArtistPopularReleases([FromRoute] Guid id, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetArtistPopularReleasesQuery(id, limit), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/albums")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedList<ArtistAlbumDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<ArtistAlbumDto>>> GetArtistAlbums([FromRoute] Guid id, [FromQuery] GetArtistAlbumsQuery query, CancellationToken ct = default)
    {
        var queryWithId = query with { ArtistId = id };
        var result = await Mediator.Send(queryWithId, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/singles")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedList<ArtistAlbumDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<ArtistAlbumDto>>> GetArtistSingles([FromRoute] Guid id, [FromQuery] GetArtistSinglesQuery query, CancellationToken ct = default)
    {
        var queryWithId = query with { ArtistId = id };
        var result = await Mediator.Send(queryWithId, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/playlists")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedList<ArtistPlaylistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<ArtistPlaylistDto>>> GetArtistPlaylists([FromRoute] Guid id, [FromQuery] GetArtistPlaylistsQuery query, CancellationToken ct = default)
    {
        var queryWithId = query with { ArtistId = id };
        var result = await Mediator.Send(queryWithId, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/related-tracks")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<RelatedTrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArtistRelatedTracks([FromRoute] Guid id, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetArtistRelatedTracksQuery(id, limit), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/similar-artists")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SimilarArtistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArtistSimilarArtists([FromRoute] Guid id, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetArtistSimilarArtistsQuery(id, limit), ct);
        return Ok(result);
    }
}
