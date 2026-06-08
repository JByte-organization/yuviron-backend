using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Complaints.Commands.ApproveComplaint;
using Yuviron.Application.Features.Admin.Complaints.Commands.RejectComplaint;
using Yuviron.Application.Features.Admin.Complaints.Queries.GetComplaintById;
using Yuviron.Application.Features.Admin.Complaints.Queries.GetComplaints;

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/complaints")]
public class AdminComplaintsController : AdminApiControllerBase
{
    
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ComplaintListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<ComplaintListItemDto>>> GetComplaints([FromQuery] GetComplaintsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplaintDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComplaintDetailsDto>> GetComplaintById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetComplaintByIdQuery(id), ct);
        return Ok(result);
    }
    
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveComplaint([FromRoute] Guid id, [FromBody] ReviewComplaintRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ApproveComplaintCommand(id, request.AdminNote), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectComplaint([FromRoute] Guid id, [FromBody] ReviewComplaintRequest request, CancellationToken ct)
    {
        await Mediator.Send(new RejectComplaintCommand(id, request.AdminNote), ct);
        return NoContent();
    }
}

public record ReviewComplaintRequest(string? AdminNote);
