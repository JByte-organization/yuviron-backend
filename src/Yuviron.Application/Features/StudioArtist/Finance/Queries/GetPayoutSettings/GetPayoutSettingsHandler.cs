using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetPayoutSettings;

public sealed class GetPayoutSettingsHandler : IRequestHandler<GetPayoutSettingsQuery, PayoutSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPayoutSettingsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PayoutSettingsDto> Handle(GetPayoutSettingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _context.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId, cancellationToken);
        
        if (!hasAccess) throw new ForbiddenException("No access to this artist's profile.");

        var settings = await _context.ArtistPayoutSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ArtistId == request.ArtistId, cancellationToken);

        return settings == null 
            ? new PayoutSettingsDto(null, null) 
            : new PayoutSettingsDto(settings.Method, settings.AccountDetails);
    }
}