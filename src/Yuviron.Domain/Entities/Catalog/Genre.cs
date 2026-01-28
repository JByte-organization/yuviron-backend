using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Genre : Entity
{
    public string Name { get; private set; } = string.Empty;

    // Навигация для связи N:N
    public virtual ICollection<TrackGenre> TrackGenres { get; private set; } = new List<TrackGenre>();

    private Genre() { }

    public Genre(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}
