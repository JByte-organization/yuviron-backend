using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Ads.Commands.RegisterClick;

public sealed class RegisterAdClickHandler : IRequestHandler<RegisterAdClickCommand>
{
    private readonly IMonetizationContext _monetizationContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public RegisterAdClickHandler(IMonetizationContext monetizationContext, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _monetizationContext = monetizationContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task Handle(RegisterAdClickCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var impression = await _monetizationContext.AdImpressions
            .Where(i => i.AdId == request.AdId && i.UserId == userId)
            .OrderByDescending(i => i.ShownAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (impression != null)
        {
            impression.MarkAsClicked(utcNow);
            await _monetizationContext.SaveChangesAsync(cancellationToken);
        }
    }
}