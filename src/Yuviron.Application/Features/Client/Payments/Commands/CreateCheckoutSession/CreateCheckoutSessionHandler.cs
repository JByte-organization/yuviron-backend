using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Payments.Commands.CreateCheckoutSession;

public sealed class CreateCheckoutSessionHandler : IRequestHandler<CreateCheckoutSessionCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly TimeProvider _timeProvider;

    public CreateCheckoutSessionHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser, 
        IPaymentService paymentService,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _timeProvider = timeProvider;
    }

    public async Task<string> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        var user = await _context.Users
            .Include(u => u.Subscriptions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null) throw new UnauthorizedAccessException();

        if (user.HasActivePremiumSubscription(_timeProvider.GetUtcNow().UtcDateTime))
        {
            throw new InvalidOperationException("You already have an active Premium subscription.");
        }

        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);
        if (plan == null) throw new NotFoundException(nameof(Plan), request.PlanId);
        
        if (plan.Type != PlanType.Listener)
        {
            throw new InvalidOperationException("This plan is not available for standard users.");
        }
        
        var result = await _paymentService.CreateCheckoutSessionAsync(
            user, 
            plan, 
            request.SuccessUrl, 
            request.CancelUrl, 
            null, 
            cancellationToken);

        return result.CheckoutUrl; 
    }
}