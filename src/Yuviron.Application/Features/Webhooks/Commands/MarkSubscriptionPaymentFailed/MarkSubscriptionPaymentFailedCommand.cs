using MediatR;

namespace Yuviron.Application.Features.Webhooks.Commands.MarkSubscriptionPaymentFailed;

public sealed record MarkSubscriptionPaymentFailedCommand(
    string StripeSubscriptionId,
    string? FailureReason
) : IRequest<Unit>;
