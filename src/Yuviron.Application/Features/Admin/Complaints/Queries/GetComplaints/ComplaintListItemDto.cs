using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Complaints.Queries.GetComplaints;

public record ComplaintListItemDto(
    Guid Id,
    ComplaintTargetType TargetType,
    Guid TargetId,
    string ReasonCode,
    ComplaintStatus Status,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    string CreatedByUserEmail
);