using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class UserSavedTrack
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid TrackId { get; set; }
    public virtual Track Track { get; set; } = null!;

    public DateTime SavedAt { get; set; }
}
