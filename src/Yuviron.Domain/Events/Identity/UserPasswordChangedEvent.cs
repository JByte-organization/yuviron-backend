using System;
using MediatR;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record UserPasswordChangedEvent(Guid UserId) : IDomainEvent;