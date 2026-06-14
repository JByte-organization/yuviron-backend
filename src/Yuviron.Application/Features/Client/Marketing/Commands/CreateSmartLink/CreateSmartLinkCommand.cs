using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Marketing.Commands.CreateSmartLink;

public record CreateSmartLinkCommand(
    SmartLinkType EntityType,
    Guid EntityId
) : IRequest<SmartLinkDto>;
