using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class TrackListenHeatmap
{
    public Guid TrackId { get; set; }
    public int SecondIndex { get; set; } // 0, 1, 2... секунда
    public int PlaysCount { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Track Track { get; set; } = null!;
}