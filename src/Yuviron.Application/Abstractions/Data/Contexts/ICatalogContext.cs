using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface ICatalogContext : IDataContext
{
    IQueryable<Artist> Artists { get; }
    IQueryable<ArtistSocialLink> ArtistSocialLinks { get; }
    IQueryable<ArtistPin> ArtistPins { get; }
    IQueryable<Album> Albums { get; }
    IQueryable<Track> Tracks { get; }
    IQueryable<Genre> Genres { get; }
    IQueryable<AlbumArtist> AlbumArtists { get; }
    IQueryable<TrackArtist> TrackArtists { get; }
    IQueryable<TrackGenre> TrackGenres { get; }
    IQueryable<TrackMood> TrackMoods { get; }
    IQueryable<ArtistTeamMember> ArtistTeamMembers { get; }
    IQueryable<Mood> Moods { get; }
}