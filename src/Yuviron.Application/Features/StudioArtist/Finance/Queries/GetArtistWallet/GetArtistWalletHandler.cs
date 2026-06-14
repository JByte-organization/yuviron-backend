using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetArtistWallet;

public sealed class GetArtistWalletHandler : IRequestHandler<GetArtistWalletQuery, ArtistWalletDto>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IMonetizationContext _monetizationContext;
    private readonly ICurrentUserService _currentUser;

    public GetArtistWalletHandler(ICatalogContext catalogContext, IMonetizationContext monetizationContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _monetizationContext = monetizationContext; _currentUser = currentUser;
    }

    public async Task<ArtistWalletDto> Handle(GetArtistWalletQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId, cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this artist's finances.");

        var wallet = await _monetizationContext.ArtistWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.ArtistId == request.ArtistId, cancellationToken);

        if (wallet == null) return new ArtistWalletDto(request.ArtistId, 0, 0, 0, DateTime.UtcNow);

        return new ArtistWalletDto(
            wallet.ArtistId, wallet.AvailableBalance, wallet.HeldBalance, wallet.TotalEarned, wallet.UpdatedAt);
    }
}