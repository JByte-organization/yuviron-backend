using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class UserSavedAlbum
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid AlbumId { get; set; }
    public virtual Album Album { get; set; } = null!;

    public DateTime SavedAt { get; set; }
}