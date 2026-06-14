using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Ads.Commands.ToggleAdStatus;

public sealed class ToggleAdStatusHandler : IRequestHandler<ToggleAdStatusCommand, Unit>
{
    private readonly IMonetizationContext _monetizationContext;
    private readonly TimeProvider _timeProvider;

    public ToggleAdStatusHandler(IMonetizationContext monetizationContext, TimeProvider timeProvider)
    {
        _monetizationContext = monetizationContext;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(ToggleAdStatusCommand request, CancellationToken cancellationToken)
    {
        var ad = await _monetizationContext.Ads
                     .FirstOrDefaultAsync(a => a.Id == request.AdId, cancellationToken)
                 ?? throw new NotFoundException(nameof(Ad), request.AdId);

        ad.SetActiveStatus(request.IsActive, _timeProvider.GetUtcNow().UtcDateTime);
        await _monetizationContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}