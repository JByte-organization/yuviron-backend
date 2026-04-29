namespace Yuviron.Application.Features.Client.Home.Queries.GetNewReleases;

public record NewReleaseDto(
    Guid Id,
    string Title,
    string ArtistNames,
    string? CoverUrl,
    int TracksCount
);