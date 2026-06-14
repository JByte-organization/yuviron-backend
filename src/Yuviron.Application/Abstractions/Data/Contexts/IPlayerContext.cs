using Yuviron.Application.Abstractions.Data;
namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IPlayerContext : IDataContext
{
    System.Linq.IQueryable<Yuviron.Domain.Entities.PlaybackSession> PlaybackSessions { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.PlaybackQueueItem> PlaybackQueueItems { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.SharedRoom> SharedRooms { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.SharedRoomMember> SharedRoomMembers { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.SharedRoomQueueItem> SharedRoomQueueItems { get; }
}