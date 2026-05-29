using MediatR;
using System;

namespace Yuviron.Application.Features.Webhooks.Commands.FulfillArtistSubscription;

public sealed record FulfillArtistSubscriptionCommand(
    Guid PayerUserId, 
    Guid ArtistId, 
    Guid PlanId, string? StripeSubscriptionId
) : IRequest<Unit>;