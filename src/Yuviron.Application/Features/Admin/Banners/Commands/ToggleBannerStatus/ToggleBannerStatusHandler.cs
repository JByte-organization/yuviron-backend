using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Commands.ToggleBannerStatus;

public sealed class ToggleBannerStatusHandler : IRequestHandler<ToggleBannerStatusCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public ToggleBannerStatusHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(ToggleBannerStatusCommand request, CancellationToken cancellationToken)
    {
        var banner = await _context.Banners
            .FirstOrDefaultAsync(b => b.Id == request.BannerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Banner), request.BannerId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        banner.ToggleStatus(utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
