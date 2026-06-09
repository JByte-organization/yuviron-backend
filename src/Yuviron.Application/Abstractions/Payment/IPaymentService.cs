
using Yuviron.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.Payment;

public record BillingInvoiceDto(string Id, decimal AmountPaid, string Currency, string Status, DateTime CreatedUrl, string HostedInvoiceUrl);


public record CheckoutSessionResult(string SessionId, string CheckoutUrl);

public interface IPaymentService
{
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        User user, 
        Plan plan, 
        string successUrl, 
        string cancelUrl, 
        Guid? artistId = null,
        CancellationToken cancellationToken = default);

    Task CancelSubscriptionAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    
    Task<CheckoutSessionResult> CreateBannerCheckoutSessionAsync(User user, BannerRequest bannerRequest, decimal amount, string currency, string successUrl, string cancelUrl, CancellationToken cancellationToken = default);
    Task RefundPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<List<BillingInvoiceDto>> GetBillingHistoryAsync(string customerEmail, CancellationToken cancellationToken = default);
}
