using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class UserSettings : Entity
{
    public Guid UserId { get; private set; } // PK + FK
    public string LanguageCode { get; private set; } = "en";
    public string ThemeMode { get; private set; } = "system"; // light, dark

    public Guid? CustomThemeId { get; private set; }

    public int AudioQualityPreference { get; private set; } = 320;
    public int CrossfadeMs { get; private set; }
    public bool PipEnabled { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual CustomTheme? CustomTheme { get; private set; }

    private UserSettings() { }
}
