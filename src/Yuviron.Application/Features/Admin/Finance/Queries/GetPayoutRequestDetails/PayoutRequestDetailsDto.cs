using Yuviron.Domain.Enums;
using Yuviron.Domain.Enums.Monetization;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetPayoutRequestDetails;

public record PayoutRequestDetailsDto(
    Guid Id,
    Guid ArtistId,
    string ArtistName,
    
    // Дані по виплаті
    decimal RequestedAmount,
    PayoutStatus Status,
    DateTime RequestedAt,
    
    // Куди платити
    PayoutMethod PayoutMethod,
    string AccountDetails,
    
    // Траст-метрики гаманця
    decimal ArtistTotalEarned,
    decimal ArtistAvailableBalance,
    decimal ArtistHeldBalance
);