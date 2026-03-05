using System;
using MediatR;

namespace Yuviron.Domain.Events;

public record AlbumDeletedEvent(Guid AlbumId) : INotification;