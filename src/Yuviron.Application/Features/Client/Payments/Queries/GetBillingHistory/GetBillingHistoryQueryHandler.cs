using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Payments.Queries.GetBillingHistory;

public class GetBillingHistoryQueryHandler : IRequestHandler<GetBillingHistoryQuery, List<BillingInvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;

    public GetBillingHistoryQueryHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser,
        IPaymentService paymentService)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
    }

    public async Task<List<BillingInvoiceDto>> Handle(GetBillingHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        return await _paymentService.GetBillingHistoryAsync(user.Email, cancellationToken);
    }
}

