using Yuviron.Domain.Enums.Monetization;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetWalletTransactionsAdmin;

public record AdminWalletTransactionDto(
    Guid Id,
    decimal Amount,
    WalletTransactionType Type,
    string Description,
    DateTime CreatedAt
);