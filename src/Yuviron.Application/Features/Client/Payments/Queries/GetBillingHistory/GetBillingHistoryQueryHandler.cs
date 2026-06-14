using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly IIdentityContext _identityContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;

    public GetBillingHistoryQueryHandler(
        IIdentityContext identityContext, 
        ICurrentUserService currentUser,
        IPaymentService paymentService)
    {
        _identityContext = identityContext;
        _currentUser = currentUser;
        _paymentService = paymentService;
    }

    public async Task<List<BillingInvoiceDto>> Handle(GetBillingHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var user = await _identityContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        return await _paymentService.GetBillingHistoryAsync(user.Email, cancellationToken);
    }
}

