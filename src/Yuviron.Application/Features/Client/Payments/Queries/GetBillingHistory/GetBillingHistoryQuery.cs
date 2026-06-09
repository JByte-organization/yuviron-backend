using MediatR;
using System.Collections.Generic;
using Yuviron.Application.Abstractions.Payment;

namespace Yuviron.Application.Features.Client.Payments.Queries.GetBillingHistory;

public record GetBillingHistoryQuery() : IRequest<List<BillingInvoiceDto>>;
