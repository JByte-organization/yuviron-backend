using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record PayoutSettingsChangedEvent(Guid ArtistId) : IDomainEvent, IArtistEvent;