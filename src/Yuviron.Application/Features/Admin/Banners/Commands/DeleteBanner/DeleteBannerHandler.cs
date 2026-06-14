using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions; 

namespace Yuviron.Application.Features.Admin.Banners.Commands.DeleteBanner;

public sealed class DeleteBannerHandler : IRequestHandler<DeleteBannerCommand, Unit>
{
    private readonly IContentContext _contentContext;

    public DeleteBannerHandler(IContentContext contentContext)
    {
        _contentContext = contentContext;
    }

    public async Task<Unit> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await _contentContext.Banners
                         .FirstOrDefaultAsync(b => b.Id == request.BannerId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Banner), request.BannerId);

        banner.Delete();

        _contentContext.Remove(banner);

        await _contentContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}