using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Yuviron.Application.Features.Client.Home.Queries.GetHomeBanners;
using Yuviron.Application.Features.Client.Home.Queries.GetNewReleases;
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopArtists;
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks;
using Yuviron.Application.Features.Client.RecentlyPlayed.Queries.GetUserRecentlyPlayed;

namespace Yuviron.Api.Controllers.Client;

[Route("api/home")]
[ApiExplorerSettings(GroupName = "client")]
public class HomeController : ApiControllerBase
{
    [HttpGet("banners")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<HomeBannerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBanners([FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetHomeBannersQuery(limit), ct);
        return Ok(result);
    }
        
    [HttpGet("top-tracks")]
    [Authorize]
    [ProducesResponseType(typeof(List<TopTrackDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopTracks([FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetUserTopTracksQuery(limit), ct);
        return Ok(result);
    }
    
    [HttpGet("new-releases")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<NewReleaseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNewReleases([FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetNewReleasesQuery(limit), ct);
        return Ok(result);
    }
    
    [HttpGet("top-artists")]
    [Authorize]
    [ProducesResponseType(typeof(List<TopArtistDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopArtists([FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetUserTopArtistsQuery(limit), ct);
        return Ok(result);
    }  
    
    [HttpGet("recently-played")]
    [Authorize]
    [ProducesResponseType(typeof(List<RecentlyPlayedTrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RecentlyPlayedTrackDto>>> GetRecentlyPlayed([FromQuery] GetUserRecentlyPlayedQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
}