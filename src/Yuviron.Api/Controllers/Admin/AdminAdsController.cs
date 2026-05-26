using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common; 
using Yuviron.Application.Features.Admin.Ads.Commands.CreateAd;
using Yuviron.Application.Features.Admin.Ads.Commands.ToggleAdStatus;
using Yuviron.Application.Features.Admin.Ads.Commands.DeleteAd;
using Yuviron.Application.Features.Admin.Ads.Commands.UpdateAd;
using Yuviron.Application.Features.Admin.Ads.Queries.GetAds; 
using Yuviron.Application.Features.Admin.Ads.Queries.GetAdById; 

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/ads")]
[ApiExplorerSettings(GroupName = "admin")]
[Authorize(Roles = "Admin")]
public class AdminAdsController : ApiControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AdSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAds([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetAdsQuery(pageNumber, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAdByIdQuery(id), ct);
        return Ok(result);
    }


    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAd([FromBody] CreateAdRequest request, CancellationToken ct)
    {
        var command = new CreateAdCommand(
            request.AdvertiserName, request.Title, request.AudioFileId, 
            request.ImageFileId, request.ClickUrl, request.IsActive);
        
        var adId = await Mediator.Send(command, ct);
        return Created($"/api/admin/ads/{adId}", adId);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAd([FromRoute] Guid id, [FromBody] UpdateAdRequest request, CancellationToken ct)
    {
        var command = new UpdateAdCommand(id, request.AdvertiserName, request.Title, request.ClickUrl);
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ToggleStatus([FromRoute] Guid id, [FromBody] bool isActive, CancellationToken ct)
    {
        await Mediator.Send(new ToggleAdStatusCommand(id, isActive), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAd([FromRoute] Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteAdCommand(id), ct);
        return NoContent();
    }
}