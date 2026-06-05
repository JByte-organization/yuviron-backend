using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Application.Configuration;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums; 

namespace Yuviron.Infrastructure.Services.Payment;

public class StripePaymentService : IPaymentService
{
    public StripePaymentService(IOptions<StripeOptions> stripeOptions)
    {
        StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;
    }

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        User user, 
        Yuviron.Domain.Entities.Plan plan, 
        string successUrl, 
        string cancelUrl, 
        Guid? artistId = null, 
        CancellationToken cancellationToken = default)
    {
        var metadata = new Dictionary<string, string>
        {
            { "PlanId", plan.Id.ToString() } 
        };

        if (artistId.HasValue)
        {
            metadata.Add("ArtistId", artistId.Value.ToString());
        }

        bool isRecurring = (plan.Period == PlanPeriod.Month || plan.Period == PlanPeriod.Year) && plan.Price > 0;

        var priceData = new SessionLineItemPriceDataOptions
        {
            UnitAmountDecimal = plan.Price * 100m, 
            Currency = plan.Currency.ToLower(),
            ProductData = new SessionLineItemPriceDataProductDataOptions
            {
                Name = plan.Name,
                Description = $"Yuviron {plan.Type} Subscription" 
            },
        };

        if (isRecurring)
        {
            priceData.Recurring = new SessionLineItemPriceDataRecurringOptions
            {
                Interval = plan.Period == PlanPeriod.Month ? "month" : "year"
            };
        }

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            CustomerEmail = user.Email,
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = priceData, 
                    Quantity = 1,
                },
            },
            Mode = isRecurring ? "subscription" : "payment", 
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = user.Id.ToString(), 
            Metadata = metadata 
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return new CheckoutSessionResult(session.Id, session.Url);
    }
    
    public async Task CancelSubscriptionAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
    {
        var service = new SubscriptionService();
        await service.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true 
        }, cancellationToken: cancellationToken);
    }
    
    public async Task<CheckoutSessionResult> CreateBannerCheckoutSessionAsync(
        User user, 
        BannerRequest bannerRequest, 
        decimal amount, 
        string currency, 
        string successUrl, 
        string cancelUrl, 
        CancellationToken cancellationToken = default)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            CustomerEmail = user.Email,
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = amount * 100m,
                        Currency = currency.ToLower(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Banner Promotion: {bannerRequest.Title}",
                            Description = "Yuviron Homepage Banner Advertisement" 
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = user.Id.ToString(), 
            Metadata = new Dictionary<string, string>
            {
                { "BannerRequestId", bannerRequest.Id.ToString() } 
            }
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return new CheckoutSessionResult(session.Id, session.Url);
    }

    public async Task RefundPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId
        };
        var service = new RefundService();
        await service.CreateAsync(options, cancellationToken: cancellationToken);
    }
}