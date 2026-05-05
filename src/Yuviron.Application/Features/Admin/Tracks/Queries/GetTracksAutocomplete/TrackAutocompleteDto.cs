using System;
using System.Collections.Generic;
using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracksAutocomplete;

public sealed record TrackAutocompleteDto(
    Guid Id,
    string Title,
    IEnumerable<SimpleArtistDto> Artists,
    string? CoverUrl   
);