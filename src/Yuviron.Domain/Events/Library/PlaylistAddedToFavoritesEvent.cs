using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record PlaylistAddedToFavoritesEvent(Guid PlaylistId, string PlaylistName, Guid PlaylistOwnerId, Guid SavedByUserId, string SavedByUserName) : IDomainEvent;
