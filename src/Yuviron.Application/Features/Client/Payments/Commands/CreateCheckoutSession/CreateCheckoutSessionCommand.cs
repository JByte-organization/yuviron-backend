using MediatR;
using System;

namespace Yuviron.Application.Features.Client.Payments.Commands.CreateCheckoutSession;

public record CreateCheckoutSessionCommand(Guid PlanId, string SuccessUrl, string CancelUrl) : IRequest<string>;