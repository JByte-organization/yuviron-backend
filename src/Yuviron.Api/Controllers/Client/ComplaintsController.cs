using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Client.Complaints.Commands.CreateComplaint;

namespace Yuviron.Api.Controllers.Client;

[Route("api/complaints")]
[ApiExplorerSettings(GroupName = "client")]
public class ComplaintsController : ApiControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateComplaintResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateComplaintResponse>> CreateComplaint([FromBody] CreateComplaintCommand command, CancellationToken ct)
    {
        var complaintId = await Mediator.Send(command, ct);
        return Ok(new CreateComplaintResponse(complaintId));
    }
}

public record CreateComplaintResponse(Guid ComplaintId);
