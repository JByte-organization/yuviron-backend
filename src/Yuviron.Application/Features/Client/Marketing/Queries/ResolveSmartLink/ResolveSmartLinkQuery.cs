using MediatR;

namespace Yuviron.Application.Features.Client.Marketing.Queries.ResolveSmartLink;

public sealed record ResolveSmartLinkResponse(string RelativePath);

public sealed record ResolveSmartLinkQuery(
    string Code,
    string? CountryCode,
    string? Referrer,
    string? DeviceType
) : IRequest<ResolveSmartLinkResponse>;