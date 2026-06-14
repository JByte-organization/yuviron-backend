using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Ads.Commands.RegisterImpression;

public sealed class RegisterAdImpressionHandler : IRequestHandler<RegisterAdImpressionCommand>
{
    private readonly IMonetizationContext _monetizationContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public RegisterAdImpressionHandler(IMonetizationContext monetizationContext, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _monetizationContext = monetizationContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task Handle(RegisterAdImpressionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException(); 
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var impression = AdImpression.Create(request.AdId, userId, request.Context, utcNow);

        _monetizationContext.Add(impression);
        await _monetizationContext.SaveChangesAsync(cancellationToken);
    }
}
