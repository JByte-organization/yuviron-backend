namespace Yuviron.Application.Abstractions.Analytics;

public sealed record TrackTrendCandidateDto(
    Guid TrackId,
    long Current24hPlays,
    long Previous24hPlays
);
