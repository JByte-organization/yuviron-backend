using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common; 
using Yuviron.Application.Features.Admin.Ads.Commands.CreateAd;
using Yuviron.Application.Features.Admin.Ads.Commands.ToggleAdStatus;
using Yuviron.Application.Features.Admin.Ads.Commands.DeleteAd;
using Yuviron.Application.Features.Admin.Ads.Commands.UpdateAd;
using Yuviron.Application.Features.Admin.Ads.Queries.GetAds; 
using Yuviron.Application.Features.Admin.Ads.Queries.GetAdById;
using Yuviron.Application.Features.Admin.Ads.Queries.GetAdAnalytics; 

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/ads")]
[ApiExplorerSettings(GroupName = "admin")]
public class AdminAdsController : AdminApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AdSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<AdSummaryDto>>> GetAds([FromQuery] GetAdsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdDetailsDto>> GetAdById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAdByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateAdResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateAdResponse>> CreateAd([FromBody] CreateAdCommand command, CancellationToken ct)
    {
        var adId = await Mediator.Send(command, ct);
        return Ok(new CreateAdResponse(adId));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAd(Guid id, [FromBody] UpdateAdCommand command, CancellationToken ct)
    {
        var commandWithId = command with { AdId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] ToggleAdStatusCommand command, CancellationToken ct)
    {
        var commandWithId = command with { AdId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAd(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteAdCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/analytics")]
    [ProducesResponseType(typeof(List<AdAnalyticsPointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AdAnalyticsPointDto>>> GetAdAnalytics(
        Guid id, 
        [FromQuery] string interval = "day", 
        [FromQuery] DateTime? minDate = null, 
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetAdAnalyticsQuery(id, interval, minDate ?? DateTime.UtcNow.AddDays(-30)), ct);
        return Ok(result);
    }
}
