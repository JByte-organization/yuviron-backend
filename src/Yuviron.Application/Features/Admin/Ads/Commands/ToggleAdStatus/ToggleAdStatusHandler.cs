using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Ads.Commands.ToggleAdStatus;

public sealed class ToggleAdStatusHandler : IRequestHandler<ToggleAdStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public ToggleAdStatusHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(ToggleAdStatusCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Ads.FindAsync(new object[] { request.AdId }, cancellationToken)
                 ?? throw new NotFoundException(nameof(Ad), request.AdId);

        ad.SetActiveStatus(request.IsActive, _timeProvider.GetUtcNow().UtcDateTime);
        await _context.SaveChangesAsync(cancellationToken);
    }
}