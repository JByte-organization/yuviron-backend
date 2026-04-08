using System;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracksAutocomplete;

public sealed record TrackAutocompleteDto(
    Guid Id,
    string Title,
    string ArtistNames,
    string? CoverUrl   
);