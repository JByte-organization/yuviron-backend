using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Finance.Commands.ApprovePayout;
using Yuviron.Application.Features.Admin.Finance.Commands.RejectPayout;
using Yuviron.Application.Features.Admin.Finance.Queries.GetPayoutRequestDetails;
using Yuviron.Application.Features.Admin.Finance.Queries.GetPayoutRequests;
using Yuviron.Application.Features.Admin.Finance.Queries.GetArtistWalletAdmin;
using Yuviron.Application.Features.Admin.Finance.Queries.GetWalletTransactionsAdmin; 

namespace Yuviron.Api.Controllers.Admin;

[Route("api/admin/finance")]
public class AdminFinanceController : AdminApiControllerBase 
{
    [HttpGet("payouts")]
    [ProducesResponseType(typeof(PaginatedList<PayoutRequestListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<PayoutRequestListItemDto>>> GetPayoutRequests([FromQuery] GetPayoutRequestsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("payouts/{id:guid}")]
    [ProducesResponseType(typeof(PayoutRequestDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PayoutRequestDetailsDto>> GetPayoutRequestDetails(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPayoutRequestDetailsQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("payouts/{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ApprovePayout(Guid id, [FromBody] ApprovePayoutCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { PayoutRequestId = id }, ct);
        return NoContent();
    }

    [HttpPost("payouts/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectPayout(Guid id, [FromBody] RejectPayoutCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { PayoutRequestId = id }, ct);
        return NoContent();
    }

    [HttpGet("wallets/{artistId:guid}")]
    [ProducesResponseType(typeof(AdminArtistWalletDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminArtistWalletDto>> GetArtistWallet(Guid artistId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetArtistWalletAdminQuery(artistId), ct);
        return Ok(result);
    }

    [HttpGet("wallets/{artistId:guid}/transactions")]
    [ProducesResponseType(typeof(PaginatedList<AdminWalletTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<AdminWalletTransactionDto>>> GetArtistWalletTransactions(
        Guid artistId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken ct = default)
    {
        var query = new GetWalletTransactionsAdminQuery(artistId, page, pageSize);
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
}