using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetPayoutRequests;

public record PayoutRequestListItemDto(
    Guid Id,
    SimpleArtistDto Artist,
    
    decimal RequestedAmount,
    decimal ArtistTotalEarned, 
    
    PayoutStatus Status,
    DateTime RequestedAt,
    DateTime UpdatedAt, 
    
    string? ProcessedByAdminName, 
    string? DecisionNote
);