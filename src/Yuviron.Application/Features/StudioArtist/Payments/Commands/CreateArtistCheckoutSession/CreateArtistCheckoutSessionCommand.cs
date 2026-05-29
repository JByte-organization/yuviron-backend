using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Payments.Commands.CreateArtistCheckoutSession;

public sealed record CreateArtistCheckoutSessionCommand(
    Guid ArtistId, 
    Guid PlanId, 
    string SuccessUrl, 
    string CancelUrl
) : IRequest<string>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}