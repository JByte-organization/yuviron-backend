using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Lyrics : Entity
{
    public Guid TrackId { get; private set; } // 1:1 с Track
    public string LanguageCode { get; private set; } = "en";
    public string PlainText { get; private set; } = string.Empty;

    public virtual Track Track { get; private set; } = null!;

    private Lyrics() { }
}
