using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Banners.Commands.CreateBanner;
using Yuviron.Application.Features.Admin.Banners.Commands.DeleteBanner;
using Yuviron.Application.Features.Admin.Banners.Commands.ToggleBannerStatus;
using Yuviron.Application.Features.Admin.Banners.Commands.UpdateBanner;
using Yuviron.Application.Features.Admin.Banners.Commands.ApproveBannerRequest;
using Yuviron.Application.Features.Admin.Banners.Commands.RejectBannerRequest;
using Yuviron.Application.Features.Admin.Banners.Queries.GetBannerRequests;
using Yuviron.Application.Features.Admin.Banners.Queries.DTOs;
using Yuviron.Application.Features.Admin.Banners.Queries.GetBannerById;
using Yuviron.Application.Features.Admin.Banners.Queries.GetBanners;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/banners")]
public class AdminBannersController : AdminApiControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<BannerListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<BannerListItemDto>>> GetBanners([FromQuery] GetBannersQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BannerDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BannerDetailsDto>> GetBannerById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetBannerByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateBannerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateBannerResponse>> CreateBanner([FromBody] CreateBannerCommand command, CancellationToken ct)
    {
        var bannerId = await Mediator.Send(command, ct);
        return Ok(new CreateBannerResponse(bannerId));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBanner(Guid id, [FromBody] UpdateBannerCommand command, CancellationToken ct)
    {
        var commandWithId = command with { BannerId = id };
        await Mediator.Send(commandWithId, ct);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/toggle-status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleBannerStatus(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new ToggleBannerStatusCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBanner(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteBannerCommand(id), ct);
        return NoContent();
    }
    

    [HttpGet("requests")]
    [ProducesResponseType(typeof(PaginatedList<BannerRequestListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<BannerRequestListItemDto>>> GetRequests([FromQuery] GetBannerRequestsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost("requests/{requestId:guid}/approve")]
    [ProducesResponseType(typeof(CreateBannerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateBannerResponse>> ApproveRequest(Guid requestId, [FromBody] ApproveBannerRequestCommand command, CancellationToken ct)
    {
        var newBannerId = await Mediator.Send(command with { RequestId = requestId }, ct);
        return Ok(new CreateBannerResponse(newBannerId));
    }

    [HttpPost("requests/{requestId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectRequest(Guid requestId, [FromBody] RejectBannerRequestCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { RequestId = requestId }, ct);
        return NoContent();
    }
}

public record CreateBannerResponse(Guid BannerId);