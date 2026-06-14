using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetPayoutRequestDetails;

public sealed class GetPayoutRequestDetailsHandler : IRequestHandler<GetPayoutRequestDetailsQuery, PayoutRequestDetailsDto>
{
    private readonly IMonetizationContext _monetizationContext;

    public GetPayoutRequestDetailsHandler(IMonetizationContext monetizationContext)
    {
        _monetizationContext = monetizationContext;
    }

    public async Task<PayoutRequestDetailsDto> Handle(GetPayoutRequestDetailsQuery request, CancellationToken cancellationToken)
    {
        var payout = await _monetizationContext.PayoutRequests
                         .Include(pr => pr.Artist)
                         .ThenInclude(a => a.ArtistWallet)
                         .Include(pr => pr.Artist)
                         .ThenInclude(a => a.PayoutSettings) 
                         .AsNoTracking()
                         .FirstOrDefaultAsync(pr => pr.Id == request.PayoutRequestId, cancellationToken)
                     ?? throw new NotFoundException(nameof(PayoutRequest), request.PayoutRequestId);

        var settings = payout.Artist.PayoutSettings 
                       ?? throw new InvalidOperationException("Artist has no payout settings configured.");
            
        var wallet = payout.Artist.ArtistWallet;

        return new PayoutRequestDetailsDto(
            payout.Id,
            payout.ArtistId,
            payout.Artist.Name,
            payout.RequestedAmount,
            payout.Status,
            payout.RequestedAt,
            settings.Method,
            settings.AccountDetails, 
            wallet?.TotalEarned ?? 0,
            wallet?.AvailableBalance ?? 0,
            wallet?.HeldBalance ?? 0
        );
    }
}