using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Client.Home.Queries.GetHomeBanners;
using Yuviron.Application.Features.Client.Home.Queries.GetNewReleases;
using Yuviron.Application.Features.Client.Home.Queries.GetSystemTopArtists; 
using Yuviron.Application.Features.Client.Home.Queries.GetSystemTopTracks;  
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopArtists; 
using Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks;
using Yuviron.Application.Features.Client.Home.Queries.GetPersonalizedRecommendations; 

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
    [AllowAnonymous] 
    [ProducesResponseType(typeof(List<TopTrackDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopTracks([FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetSystemTopTracksQuery(limit), ct);
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
    [AllowAnonymous] 
    [ProducesResponseType(typeof(List<TopArtistDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopArtists([FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetSystemTopArtistsQuery(limit), ct);
        return Ok(result);
    }  

    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(List<RecommendationTrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RecommendationTrackDto>>> GetPersonalizedRecommendations([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetPersonalizedRecommendationsQuery(limit), ct);
        return Ok(result);
    }
}
