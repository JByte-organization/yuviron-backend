using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Client.Tracks.Queries.GetTrackById;
using Yuviron.Application.Features.Client.Tracks.Queries.GetTrackRecommendations;
using Yuviron.Application.Features.Client.Tracks.Queries.GetTrackStreamUrl;

namespace Yuviron.Api.Controllers.Client;

[Route("api/tracks")]
[ApiExplorerSettings(GroupName = "client")]
public class TracksController : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TrackDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrackById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetTrackByIdQuery(id), ct);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}/recommendations")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<RecommendedTrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrackRecommendations(
        [FromRoute] Guid id, 
        [FromQuery] int limit = 10, 
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetTrackRecommendationsQuery(id, limit), ct);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}/play")]
    [Authorize] 
    [ProducesResponseType(typeof(TrackStreamUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrackStreamUrl([FromRoute] Guid id, CancellationToken ct = default)
    {
        // 🛡️ ДОБАВЛЕНО: Получаем IP-адрес для привязки HLS-токена
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

        var result = await Mediator.Send(new GetTrackStreamUrlQuery(id, ipAddress), ct);
        return Ok(result);
    }
    
    [HttpGet("debug-ip")]
    [AllowAnonymous]
    public IActionResult DebugIp()
    {
        return Ok(new
        {
            RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            XForwardedFor = Request.Headers["X-Forwarded-For"].ToString(),
            XRealIp = Request.Headers["X-Real-IP"].ToString(),
            CFConnectingIp = Request.Headers["CF-Connecting-IP"].ToString()
        });
    }
}