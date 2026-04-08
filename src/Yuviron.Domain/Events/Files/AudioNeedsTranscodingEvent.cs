using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record AudioNeedsTranscodingEvent(
    Guid TrackId, 
    string TempAudioStorageKey
) : IDomainEvent;