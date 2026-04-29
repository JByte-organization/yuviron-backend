using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions; 

namespace Yuviron.Application.Features.Admin.Banners.Commands.DeleteBanner;

public sealed class DeleteBannerHandler : IRequestHandler<DeleteBannerCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteBannerHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await _context.Banners
                         .FirstOrDefaultAsync(b => b.Id == request.BannerId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Banner), request.BannerId);

        banner.Delete();

        _context.Banners.Remove(banner);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}