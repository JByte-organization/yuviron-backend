using MediatR;

namespace Yuviron.Application.Features.Webhooks.Commands.FulfillBannerPayment;

public sealed record FulfillBannerPaymentCommand(Guid BannerRequestId, string PaymentIntentId) : IRequest<Unit>;