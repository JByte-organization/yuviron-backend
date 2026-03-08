using System;
using MediatR;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Events;

public record GenreDeletedEvent(Guid GenreId) : IDomainEvent;