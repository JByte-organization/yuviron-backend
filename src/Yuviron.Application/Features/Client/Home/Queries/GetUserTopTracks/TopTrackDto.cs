namespace Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks;

public record TopTrackDto(
    Guid Id,
    string Title,
    string ArtistNames,
    string? CoverUrl
);