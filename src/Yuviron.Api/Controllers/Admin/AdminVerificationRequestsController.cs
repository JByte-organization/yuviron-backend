using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.VerificationRequests.Commands.ApproveRequest;
using Yuviron.Application.Features.Admin.VerificationRequests.Commands.RejectRequest;
using Yuviron.Application.Features.Admin.VerificationRequests.Queries.GetRequestById;
using Yuviron.Application.Features.Admin.VerificationRequests.Queries.GetRequests;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/verification-requests")]
public class AdminVerificationRequestsController : AdminApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<VerificationRequestListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<VerificationRequestListItemDto>>> GetRequests([FromQuery] GetVerificationRequestsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VerificationRequestDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerificationRequestDetailDto>> GetRequestById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetVerificationRequestByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveRequest([FromRoute] Guid id, [FromBody] ApproveRequestDto request, CancellationToken ct)
    {
        var command = new ApproveVerificationRequestCommand(id, request.AdminNote);
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectRequest([FromRoute] Guid id, [FromBody] RejectRequestDto request, CancellationToken ct)
    {
        var command = new RejectVerificationRequestCommand(id, request.AdminNote);
        await Mediator.Send(command, ct);
        return NoContent();
    }
    
    public record ApproveRequestDto(string? AdminNote);
    public record RejectRequestDto(string? AdminNote);
}