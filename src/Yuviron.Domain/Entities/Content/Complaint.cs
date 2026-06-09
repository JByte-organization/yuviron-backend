using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;


public class Complaint : Entity
{
    public Guid CreatedByUserId { get; private set; }
    public ComplaintTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public string ReasonCode { get; private set; } = string.Empty;
    public string? Comment { get; private set; }

    public ComplaintStatus Status { get; private set; }
    public Guid? ModeratedByAdminId { get; private set; }
    public string? ModerationNote { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual User CreatedByUser { get; private set; } = null!;
    public virtual User? ModeratedByAdmin { get; private set; }

    private Complaint() { }

    public static Complaint Create(
        Guid userId, ComplaintTargetType targetType, Guid targetId,
        ComplaintReasonCode reasonCode, string? comment, DateTime utcNow)
    {
        return new Complaint
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = userId,
            TargetType = targetType,
            TargetId = targetId,
            ReasonCode = reasonCode.ToString(),
            Comment = comment?.Trim(),
            Status = ComplaintStatus.New,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void MarkAsInReview(Guid adminId, DateTime utcNow)
    {
        if (Status != ComplaintStatus.New) throw new InvalidOperationException("Only new complaints can be reviewed.");
        Status = ComplaintStatus.InReview;
        ModeratedByAdminId = adminId;
        UpdatedAt = utcNow;
    }

    public void Approve(Guid adminId, string? note, DateTime utcNow)
    {
        Status = ComplaintStatus.Approved;
        ModeratedByAdminId = adminId;
        ModerationNote = note?.Trim();
        UpdatedAt = utcNow;
    }

    public void Reject(Guid adminId, string? note, DateTime utcNow)
    {
        Status = ComplaintStatus.Rejected;
        ModeratedByAdminId = adminId;
        ModerationNote = note?.Trim();
        UpdatedAt = utcNow;
    }
}
