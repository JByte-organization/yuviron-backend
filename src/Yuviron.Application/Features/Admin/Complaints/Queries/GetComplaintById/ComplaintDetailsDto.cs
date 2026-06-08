using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Complaints.Queries.GetComplaintById;

public record ComplaintDetailsDto(
    Guid Id,
    Guid CreatedByUserId,
    string CreatedByUserEmail,
    ComplaintTargetType TargetType,
    Guid TargetId,
    string ReasonCode,
    string? Comment,
    ComplaintStatus Status,
    Guid? ModeratedByAdminId,
    string? ModerationNote,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int TotalComplaintsForTarget // Витягуємо з ComplaintCounters
);