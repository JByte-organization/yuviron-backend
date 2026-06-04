using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Events;

public sealed record NewReleasePublishedEvent(Guid ArtistId, string ArtistName, Guid EntityId, NotificationEntityType EntityType, string Title, string? CoverUrl) : IDomainEvent, IArtistEvent;