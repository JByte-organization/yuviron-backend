using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Complaints.Commands.ApproveComplaint;

public sealed record ApproveComplaintCommand(Guid ComplaintId, string? AdminNote) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
