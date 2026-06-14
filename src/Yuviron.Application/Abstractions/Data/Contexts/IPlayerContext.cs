using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IPlayerContext : IDataContext
{
    IQueryable<PlaybackSession> PlaybackSessions { get; }
    IQueryable<PlaybackQueueItem> PlaybackQueueItems { get; }
    IQueryable<SharedRoom> SharedRooms { get; }
    IQueryable<SharedRoomMember> SharedRoomMembers { get; }
    IQueryable<SharedRoomQueueItem> SharedRoomQueueItems { get; }
}