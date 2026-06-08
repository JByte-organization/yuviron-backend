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
using Yuviron.Application.Features.Webhooks.Commands.MarkSubscriptionPaymentFailed;
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
            else if (stripeEvent.Type == "invoice.payment_failed")
            {
                using var jsonDoc = JsonDocument.Parse(jsonPayload);
                var stripeObject = jsonDoc.RootElement.GetProperty("data").GetProperty("object");

                if (stripeObject.TryGetProperty("subscription", out var subProp))
                {
                    var stripeSubId = subProp.GetString();
                    if (!string.IsNullOrEmpty(stripeSubId))
                    {
                        var failureReason = TryGetString(stripeObject, "billing_reason")
                            ?? TryGetNestedString(stripeObject, "last_payment_error", "message")
                            ?? TryGetNestedString(stripeObject, "payment_intent", "last_payment_error", "message");

                        _logger.LogInformation("Subscription payment failed for Stripe Sub: {SubId}", stripeSubId);
                        await _mediator.Send(new MarkSubscriptionPaymentFailedCommand(stripeSubId, failureReason), cancellationToken);
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

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
    }

    private static string? TryGetNestedString(JsonElement element, params string[] path)
    {
        var current = element;

        foreach (var segment in path)
        {
            if (!current.TryGetProperty(segment, out var next))
            {
                return null;
            }

            current = next;
        }

        return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
    }
}
