using Yuviron.Domain.Enums.Monetization;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetWalletTransactions;

public record WalletTransactionDto(
    Guid Id,
    decimal Amount,
    WalletTransactionType Type,
    string Description,
    DateTime CreatedAt
);