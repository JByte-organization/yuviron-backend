using MediatR;
using Microsoft.Extensions.Caching.Distributed; 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Payment; 
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Webhooks.Commands.FulfillSubscription;
using Yuviron.Application.Features.Webhooks.Commands.FulfillArtistSubscription;
using Yuviron.Application.Features.Webhooks.Commands.FulfillBannerPayment;
using Yuviron.Application.Features.Webhooks.Commands.RenewSubscription;

namespace Yuviron.Infrastructure.Services.Payment;

public class StripeWebhookParser : IStripeWebhookParser
{
    private readonly IMediator _mediator;
    private readonly string _webhookSecret;
    private readonly ILogger<StripeWebhookParser> _logger;
    private readonly IDistributedCache _cache; 

    public StripeWebhookParser(
        IMediator mediator, 
        IOptions<StripeOptions> stripeOptions,
        ILogger<StripeWebhookParser> logger,
        IDistributedCache cache)
    {
        _mediator = mediator;
        _webhookSecret = stripeOptions.Value.WebhookSecret;
        _logger = logger;
        _cache = cache;
    }

    public async Task ProcessWebhookAsync(string jsonPayload, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(jsonPayload, signature, _webhookSecret);

            var cacheKey = $"stripe_event:{stripeEvent.Id}";
            var alreadyProcessed = await _cache.GetStringAsync(cacheKey, cancellationToken);
            
            if (!string.IsNullOrEmpty(alreadyProcessed))
            {
                _logger.LogInformation("Stripe webhook {EventId} was already processed. Skipping to prevent double-processing.", stripeEvent.Id);
                return; 
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                if (stripeEvent.Data.Object is Stripe.Checkout.Session session && session.PaymentStatus == "paid")
                {
                    if (session.Metadata.TryGetValue("BannerRequestId", out var bannerReqIdStr) && Guid.TryParse(bannerReqIdStr, out var bannerReqId))
                    {
                        _logger.LogInformation("Banner Payment success received for Request {BannerReqId}", bannerReqId);
                        await _mediator.Send(new FulfillBannerPaymentCommand(bannerReqId, session.PaymentIntentId), cancellationToken);
                    }
                    else if (session.Metadata.TryGetValue("PlanId", out var planIdStr) && Guid.TryParse(planIdStr, out var planId))
                    {
                        if (!Guid.TryParse(session.ClientReferenceId, out var userId))
                        {
                            _logger.LogWarning("Missing or invalid ClientReferenceId in Stripe Session.");
                            return;
                        }

                        var stripeSubId = session.SubscriptionId; 

                        if (session.Metadata.TryGetValue("ArtistId", out var artistIdStr) && Guid.TryParse(artistIdStr, out var artistId))
                        {
                            _logger.LogInformation("Artist Payment success received for User {UserId}, Artist {ArtistId}, Plan {PlanId}", userId, artistId, planId);
                            await _mediator.Send(new FulfillArtistSubscriptionCommand(userId, artistId, planId, stripeSubId), cancellationToken);
                        }
                        else
                        {
                            _logger.LogInformation("Payment success received for User {UserId}, Plan {PlanId}", userId, planId);
                            await _mediator.Send(new FulfillSubscriptionCommand(userId, planId, stripeSubId), cancellationToken);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Unknown checkout session completed metadata configuration: {SessionId}", session.Id);
                    }
                }
            }
            else if (stripeEvent.Type == "invoice.payment_succeeded")
            {
                using var jsonDoc = JsonDocument.Parse(jsonPayload);
                var stripeObject = jsonDoc.RootElement.GetProperty("data").GetProperty("object");

                if (stripeObject.TryGetProperty("billing_reason", out var reasonProp) && 
                    stripeObject.TryGetProperty("subscription", out var subProp))
                {
                    var billingReason = reasonProp.GetString();
                    var stripeSubId = subProp.GetString();
                    
                    if (billingReason == "subscription_cycle" && !string.IsNullOrEmpty(stripeSubId))
                    {
                        _logger.LogInformation("Auto-renewal successful for Stripe Sub: {SubId}", stripeSubId);
                        await _mediator.Send(new RenewSubscriptionCommand(stripeSubId), cancellationToken);
                    }
                }
            }

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) };
            await _cache.SetStringAsync(cacheKey, "processed", cacheOptions, cancellationToken);
        }
        catch (StripeException e)
        {
            _logger.LogWarning(e, "Invalid Stripe webhook signature.");
            throw new UnauthorizedAccessException("Invalid signature."); 
        }
    }
}
