using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class LyricsSegment : Entity
{
    public Guid TrackId { get; private set; }
    public int StartMs { get; private set; }
    public int EndMs { get; private set; }
    public string Text { get; private set; } = string.Empty;

    public virtual Track Track { get; private set; } = null!;

    private LyricsSegment() { }
}
