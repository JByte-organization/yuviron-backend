using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Common;
using Yuviron.Application.Features.StudioArtist.Finance.Commands.RequestPayout;
using Yuviron.Application.Features.StudioArtist.Finance.Commands.UpdatePayoutSettings;
using Yuviron.Application.Features.StudioArtist.Finance.Queries.GetArtistWallet;
using Yuviron.Application.Features.StudioArtist.Finance.Queries.GetWalletTransactions;
using Yuviron.Application.Features.StudioArtist.Finance.Queries.GetPayoutSettings; // <-- Додано
using Yuviron.Application.Features.StudioArtist.Finance.Queries.GetArtistPayoutRequests; // <-- Додано

namespace Yuviron.Api.Controllers.StudioArtist;

[Route("api/studio-artist/finance")]
public class StudioArtistFinanceController : StudioArtistApiControllerBase
{
    [HttpGet("wallet")]
    [ProducesResponseType(typeof(ArtistWalletDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ArtistWalletDto>> GetWallet([FromQuery] Guid artistId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetArtistWalletQuery(artistId), ct);
        return Ok(result);
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(PaginatedList<WalletTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<WalletTransactionDto>>> GetTransactions([FromQuery] GetWalletTransactionsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(PayoutSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PayoutSettingsDto>> GetSettings([FromQuery] Guid artistId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPayoutSettingsQuery(artistId), ct);
        return Ok(result);
    }

    [HttpPost("settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdatePayoutSettingsCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return Ok();
    }

    [HttpPost("payouts")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestPayout([FromBody] RequestPayoutCommand command, CancellationToken ct)
    {
        var payoutId = await Mediator.Send(command, ct);
        return Ok(new { PayoutRequestId = payoutId });
    }

    [HttpGet("payouts")]
    [ProducesResponseType(typeof(PaginatedList<ArtistPayoutRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<ArtistPayoutRequestDto>>> GetPayoutRequests([FromQuery] GetArtistPayoutRequestsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
}