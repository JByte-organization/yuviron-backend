using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface ILibraryContext : IDataContext
{
    IQueryable<Playlist> Playlists { get; }
    IQueryable<PlaylistTrack> PlaylistTracks { get; }
    IQueryable<UserSavedTrack> UserSavedTracks { get; }
    IQueryable<UserSavedAlbum> UserSavedAlbums { get; }
    IQueryable<UserFollowArtist> UserFollowArtists { get; }
    IQueryable<UserFollowUser> UserFollowUsers { get; }
    IQueryable<UserSavedPlaylist> UserSavedPlaylists { get; }
}