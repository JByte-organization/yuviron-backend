using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetArtistWalletAdmin;

public sealed class GetArtistWalletAdminHandler : IRequestHandler<GetArtistWalletAdminQuery, AdminArtistWalletDto>
{
    private readonly IMonetizationContext _monetizationContext;

    public GetArtistWalletAdminHandler(IMonetizationContext monetizationContext)
    {
        _monetizationContext = monetizationContext;
    }

    public async Task<AdminArtistWalletDto> Handle(GetArtistWalletAdminQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _monetizationContext.ArtistWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.ArtistId == request.ArtistId, cancellationToken);

        if (wallet == null) return new AdminArtistWalletDto(request.ArtistId, 0, 0, 0, DateTime.UtcNow);

        return new AdminArtistWalletDto(
            wallet.ArtistId, wallet.AvailableBalance, wallet.HeldBalance, wallet.TotalEarned, wallet.UpdatedAt);
    }
}