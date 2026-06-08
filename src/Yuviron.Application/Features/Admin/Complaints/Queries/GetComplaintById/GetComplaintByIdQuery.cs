using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Complaints.Queries.GetComplaintById;

public sealed record GetComplaintByIdQuery(Guid ComplaintId) : IRequest<ComplaintDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}