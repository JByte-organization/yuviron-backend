using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class AdImpression : Entity
{
    public Guid AdId { get; private set; }
    public Guid? UserId { get; private set; } // Nullable, если слушает гость
    public DateTime ShownAt { get; private set; }
    public string? Context { get; private set; } // Где показали (MainScreen, Playlist...)

    public virtual Ad Ad { get; private set; } = null!;
    public virtual User? User { get; private set; }

    private AdImpression() { }
}
