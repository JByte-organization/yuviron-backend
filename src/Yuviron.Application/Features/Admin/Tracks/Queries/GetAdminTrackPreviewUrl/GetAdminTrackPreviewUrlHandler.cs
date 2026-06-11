using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetAdminTrackPreviewUrl;

public sealed class GetAdminTrackPreviewUrlHandler : IRequestHandler<GetAdminTrackPreviewUrlQuery, AdminTrackPreviewUrlResponse>
{
    // Sentinel quality value that distinguishes admin preview tokens from regular stream tokens.
    // This prevents a preview token from being replayed against the HLS stream endpoint.
    internal const int PreviewQualitySentinel = -1;
    private const int TokenLifetimeMinutes = 15;

    private readonly IApplicationDbContext _context;
    private readonly IStreamTokenService _streamTokenService;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public GetAdminTrackPreviewUrlHandler(
        IApplicationDbContext context,
        IStreamTokenService streamTokenService,
        ICurrentUserService currentUser,
        TimeProvider timeProvider)
    {
        _context = context;
        _streamTokenService = streamTokenService;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<AdminTrackPreviewUrlResponse> Handle(GetAdminTrackPreviewUrlQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var exists = await _context.Tracks
            .AsNoTracking()
            .AnyAsync(t => t.Id == request.TrackId, cancellationToken);

        if (!exists)
            throw new NotFoundException(nameof(Track), request.TrackId);

        var expiration = _timeProvider.GetUtcNow().AddMinutes(TokenLifetimeMinutes);
        var sig = _streamTokenService.GenerateToken(request.TrackId, PreviewQualitySentinel, expiration, userId);
        var expUnix = expiration.ToUnixTimeSeconds();

        var url = $"/api/admin/tracks/{request.TrackId}/audio?exp={expUnix}&uid={userId:N}&sig={sig}";

        return new AdminTrackPreviewUrlResponse(url);
    }
}
