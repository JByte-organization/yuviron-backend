using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class VerificationRequest : Entity
{
    public Guid ArtistId { get; private set; }
    public Guid SubmittedByUserId { get; private set; }

    public ClaimRole ClaimedRole { get; private set; }
    public string OfficialEmail { get; private set; } = string.Empty;
    public string Links { get; private set; } = string.Empty;
    public string? ProofFileUrl { get; private set; } 
    public string? Message { get; private set; }

    public VerificationRequestStatus Status { get; private set; }
    
    public Guid? AdminId { get; private set; }
    public string? AdminNote { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual Artist Artist { get; private set; } = null!;
    public virtual User SubmittedByUser { get; private set; } = null!;
    public virtual User? Admin { get; private set; }

    private VerificationRequest() { }

    public static VerificationRequest Create(
        Guid artistId, 
        Guid userId, 
        ClaimRole claimedRole,
        string officialEmail,
        string links, 
        string? proofFileUrl,
        string? message, 
        DateTime utcNow)
    {
        return new VerificationRequest
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            SubmittedByUserId = userId,
            ClaimedRole = claimedRole,
            OfficialEmail = officialEmail.Trim(),
            Links = links.Trim(),
            ProofFileUrl = proofFileUrl?.Trim(),
            Message = message?.Trim(),
            Status = VerificationRequestStatus.Pending, 
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void Approve(Guid adminId, string? note, DateTime utcNow)
    {
        Status = VerificationRequestStatus.Approved; 
        AdminId = adminId;
        AdminNote = note?.Trim();
        UpdatedAt = utcNow;
    }

    public void Reject(Guid adminId, string? note, DateTime utcNow)
    {
        Status = VerificationRequestStatus.Rejected; 
        AdminId = adminId;
        AdminNote = note?.Trim();
        UpdatedAt = utcNow;
    }
}