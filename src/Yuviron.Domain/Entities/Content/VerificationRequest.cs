using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class VerificationRequest : Entity
{
    public Guid ArtistId { get; private set; }
    public Guid SubmittedByUserId { get; private set; }

    public VerificationStatus Status { get; private set; }
    public Guid? AdminId { get; private set; }
    public string? AdminNote { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual Artist Artist { get; private set; } = null!;
    public virtual User SubmittedByUser { get; private set; } = null!;
    public virtual User? Admin { get; private set; }

    private VerificationRequest() { }
}
