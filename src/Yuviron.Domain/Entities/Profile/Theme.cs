using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Theme : Entity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }
    public bool IsPremiumOnly { get; private set; }

    private Theme() { }
}
