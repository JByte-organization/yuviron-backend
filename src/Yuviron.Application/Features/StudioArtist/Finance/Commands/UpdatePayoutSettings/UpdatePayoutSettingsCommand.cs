using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Enums.Monetization;

namespace Yuviron.Application.Features.StudioArtist.Finance.Commands.UpdatePayoutSettings;

public sealed record UpdatePayoutSettingsCommand(
    Guid ArtistId,
    PayoutMethod Method,
    string AccountDetails
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}