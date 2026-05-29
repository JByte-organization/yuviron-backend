using MediatR;

namespace Yuviron.Application.Features.Webhooks.Commands.FulfillSubscription;

public sealed record FulfillSubscriptionCommand(Guid UserId, Guid PlanId, string? StripeSubscriptionId) : IRequest<Unit>;