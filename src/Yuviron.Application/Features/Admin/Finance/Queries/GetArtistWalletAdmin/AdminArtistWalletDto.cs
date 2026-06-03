namespace Yuviron.Application.Features.Admin.Finance.Queries.GetArtistWalletAdmin;

public record AdminArtistWalletDto(
    Guid ArtistId,
    decimal AvailableBalance,
    decimal HeldBalance,
    decimal TotalEarned,
    DateTime UpdatedAt
);