using Yuviron.Application.Abstractions.Data;
namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface ILibraryContext : IDataContext
{
    System.Linq.IQueryable<Yuviron.Domain.Entities.Playlist> Playlists { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.PlaylistTrack> PlaylistTracks { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserSavedTrack> UserSavedTracks { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserSavedAlbum> UserSavedAlbums { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserFollowArtist> UserFollowArtists { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserFollowUser> UserFollowUsers { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserSavedPlaylist> UserSavedPlaylists { get; }
}