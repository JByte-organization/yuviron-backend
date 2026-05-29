
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Payment;

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
}