using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Queries.GetRequestById;

public sealed record GetVerificationRequestByIdQuery(
    Guid RequestId
) : IRequest<VerificationRequestDetailDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}