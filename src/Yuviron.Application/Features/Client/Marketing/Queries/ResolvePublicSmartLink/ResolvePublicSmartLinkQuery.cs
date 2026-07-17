using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Marketing.Queries.ResolvePublicSmartLink;

public sealed record ResolvePublicSmartLinkResponse(string RelativePath);

public sealed record ResolvePublicSmartLinkQuery(
    SmartLinkType EntityType,
    string PublicId,
    string? Code,
    string? CountryCode,
    string? Referrer,
    string? DeviceType
) : IRequest<ResolvePublicSmartLinkResponse>;
