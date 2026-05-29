using MediatR;

namespace Yuviron.Application.Features.Webhooks.Commands.RenewSubscription;

public sealed record RenewSubscriptionCommand(string StripeSubscriptionId) : IRequest<Unit>;