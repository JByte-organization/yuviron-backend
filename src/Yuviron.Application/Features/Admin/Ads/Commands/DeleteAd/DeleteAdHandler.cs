using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Ads.Commands.DeleteAd;

public sealed class DeleteAdHandler : IRequestHandler<DeleteAdCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeleteAdHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteAdCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Ads
                     .FirstOrDefaultAsync(a => a.Id == request.AdId, cancellationToken)
                 ?? throw new NotFoundException(nameof(Ad), request.AdId);

        ad.Delete(_timeProvider.GetUtcNow().UtcDateTime);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}