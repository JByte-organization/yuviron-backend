using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public sealed record UserFollowedUserEvent(Guid FollowerId, Guid TargetUserId, string FollowerName) : IDomainEvent;
