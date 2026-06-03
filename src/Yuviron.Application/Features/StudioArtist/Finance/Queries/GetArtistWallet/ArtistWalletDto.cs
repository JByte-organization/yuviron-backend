using System;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetArtistWallet;

public record ArtistWalletDto(
    Guid ArtistId,
    decimal AvailableBalance,
    decimal HeldBalance,
    decimal TotalEarned,
    DateTime UpdatedAt
);