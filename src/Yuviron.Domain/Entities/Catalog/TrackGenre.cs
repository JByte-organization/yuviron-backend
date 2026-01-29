using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class TrackGenre
{
    public Guid TrackId { get; set; }
    public virtual Track Track { get; set; } = null!;

    public Guid GenreId { get; set; }
    public virtual Genre Genre { get; set; } = null!;
}
