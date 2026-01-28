using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum ComplaintTargetType { Track = 1, Album = 2, Artist = 3 }
public enum ComplaintStatus { New = 1, InReview = 2, Approved = 3, Rejected = 4 }

public class Complaint : Entity
{
    public Guid CreatedByUserId { get; private set; }

    // Полиморфная цель
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
}
