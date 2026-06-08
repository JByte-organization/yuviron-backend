using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Complaints.Commands.RejectComplaint;

public sealed record RejectComplaintCommand(Guid ComplaintId, string? AdminNote) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
