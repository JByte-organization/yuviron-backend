using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetStudioArtistStats;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackRetention;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackPlaysOverTime;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistAudience;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistPlaysOverTime;

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist")] 
[Authorize] 
public class StudioArtistAnalyticsController : StudioArtistApiControllerBase
{
    // 1. Базова статистика по артисту (MySQL + Redis)
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ArtistAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtistAnalyticsDto>> GetStats(
        [FromQuery] Guid artistId, 
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetStudioArtistStatsQuery(artistId), ct);
        return Ok(result);
    }

    // 2. Утримання аудиторії конкретного треку (ClickHouse)
    [HttpGet("tracks/{trackId}/analytics/retention")]
    [ProducesResponseType(typeof(List<TrackRetentionPointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TrackRetentionPointDto>>> GetTrackRetention(
        [FromQuery] Guid artistId, 
        [FromRoute] Guid trackId, 
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetTrackRetentionQuery(artistId, trackId), ct);
        return Ok(result);
    }

    // 3. Динаміка прослуховувань конкретного треку (ClickHouse)
    [HttpGet("tracks/{trackId}/analytics/plays-over-time")]
    [ProducesResponseType(typeof(List<PlaysOverTimePointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PlaysOverTimePointDto>>> GetTrackPlaysOverTime(
        [FromQuery] Guid artistId, 
        [FromRoute] Guid trackId, 
        [FromQuery] int days = 30, 
        CancellationToken ct = default) 
    {
        var result = await Mediator.Send(new GetTrackPlaysOverTimeQuery(artistId, trackId, days), ct);
        return Ok(result);
    }

    [HttpGet("{artistId}/audience")]
    [ProducesResponseType(typeof(ArtistAudienceDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtistAudienceDashboardDto>> GetArtistAudience(
        [FromRoute] Guid artistId, 
        [FromQuery] int days = 30, 
        CancellationToken ct = default) 
    {
        var result = await Mediator.Send(new GetArtistAudienceQuery(artistId, days), ct);
        return Ok(result);
    }

    [HttpGet("{artistId}/plays-over-time")]
    [ProducesResponseType(typeof(List<PlaysOverTimePointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PlaysOverTimePointDto>>> GetArtistPlaysOverTime(
        [FromRoute] Guid artistId, 
        [FromQuery] int days = 30, 
        CancellationToken ct = default) 
    {
        var result = await Mediator.Send(new GetArtistPlaysOverTimeQuery(artistId, days), ct);
        return Ok(result);
    }
}