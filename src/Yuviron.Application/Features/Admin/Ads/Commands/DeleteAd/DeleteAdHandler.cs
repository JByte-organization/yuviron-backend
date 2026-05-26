using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Ads.Commands.DeleteAd;

public sealed class DeleteAdHandler : IRequestHandler<DeleteAdCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeleteAdHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteAdCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Ads.FindAsync(new object[] { request.AdId }, cancellationToken)
                 ?? throw new NotFoundException(nameof(Ad), request.AdId);

        ad.Delete(_timeProvider.GetUtcNow().UtcDateTime);
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}