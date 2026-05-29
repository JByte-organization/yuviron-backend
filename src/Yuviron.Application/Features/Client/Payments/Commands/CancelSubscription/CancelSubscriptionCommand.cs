using MediatR;

namespace Yuviron.Application.Features.StudioArtist.Payments.Commands.CancelSubscription;

public sealed record CancelSubscriptionCommand() : IRequest<Unit>;