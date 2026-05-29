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

namespace Yuviron.Application.Features.Client.Payments.Commands.CreateArtistCheckoutSession;

public sealed class CreateArtistCheckoutSessionHandler : IRequestHandler<CreateArtistCheckoutSessionCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly TimeProvider _timeProvider;

    public CreateArtistCheckoutSessionHandler(
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

    public async Task<string> Handle(CreateArtistCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) throw new UnauthorizedAccessException();

        var ownsArtist = await _context.Artists.AnyAsync(a => a.Id == request.ArtistId && a.TeamMembers.Any(tm => tm.UserId == userId), cancellationToken);
        if (!ownsArtist)
        {
            throw new InvalidOperationException("You do not own this artist profile.");
        }

        var hasActiveSub = await _context.ArtistSubscriptions
            .AnyAsync(s => s.ArtistId == request.ArtistId && s.Status == SubscriptionStatus.Active, cancellationToken);
            
        if (hasActiveSub)
        {
            throw new InvalidOperationException("This artist already has an active Premium subscription.");
        }

        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);
        if (plan == null) throw new NotFoundException(nameof(Plan), request.PlanId);

        if (plan.Type != PlanType.Artist)
        {
            throw new InvalidOperationException("This plan is not available for artists. Please select an Artist plan.");
        }

        var result = await _paymentService.CreateCheckoutSessionAsync(
            user, 
            plan, 
            request.SuccessUrl, 
            request.CancelUrl, 
            request.ArtistId, 
            cancellationToken);

        return result.CheckoutUrl; 
    }
}