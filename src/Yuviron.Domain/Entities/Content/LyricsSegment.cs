using System;
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

    public static LyricsSegment Create(Guid trackId, int startMs, int endMs, string text)
    {
        if (startMs < 0) throw new ArgumentException("Start time cannot be negative");
        if (endMs <= startMs) throw new ArgumentException("End time must be greater than start time");
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Segment text cannot be empty");

        return new LyricsSegment
        {
            Id = Guid.NewGuid(),
            TrackId = trackId,
            StartMs = startMs,
            EndMs = endMs,
            Text = text.Trim()
        };
    }
}