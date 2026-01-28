using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class CustomTheme : Entity
{
    public Guid UserId { get; private set; }
    public string PrimaryColor { get; private set; } = string.Empty;
    public string SecondaryColor { get; private set; } = string.Empty;
    public string BackgroundColor { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;

    private CustomTheme() { }
}
