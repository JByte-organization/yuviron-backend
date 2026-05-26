using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Ads.Commands.UpdateAd;

public sealed class UpdateAdHandler : IRequestHandler<UpdateAdCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateAdHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(UpdateAdCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Ads.FindAsync(new object[] { request.AdId }, cancellationToken)
                 ?? throw new NotFoundException(nameof(Ad), request.AdId);

        ad.UpdateDetails(
            request.AdvertiserName, 
            request.Title, 
            request.ClickUrl, 
            _timeProvider.GetUtcNow().UtcDateTime);

        await _context.SaveChangesAsync(cancellationToken);
    }
}