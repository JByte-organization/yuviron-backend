using Yuviron.Application.Abstractions.Data;
namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface ICatalogContext : IDataContext
{
    System.Linq.IQueryable<Yuviron.Domain.Entities.Artist> Artists { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ArtistSocialLink> ArtistSocialLinks { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ArtistPin> ArtistPins { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Album> Albums { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Track> Tracks { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Genre> Genres { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.AlbumArtist> AlbumArtists { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.TrackArtist> TrackArtists { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.TrackGenre> TrackGenres { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.TrackMood> TrackMoods { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ArtistTeamMember> ArtistTeamMembers { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Mood> Moods { get; }
}