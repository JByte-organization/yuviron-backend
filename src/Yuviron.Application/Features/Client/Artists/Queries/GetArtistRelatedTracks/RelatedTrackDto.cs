using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistRelatedTracks;

public sealed record RelatedTrackDto(
    Guid Id,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    string? AudioUrl,
    IEnumerable<TrackArtistDto> Artists
);
