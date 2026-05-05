using System;
using System.Collections.Generic;
using Yuviron.Application.Common.Models; 

namespace Yuviron.Application.Features.Client.Home.Queries.GetUserTopTracks;

public record TopTrackDto(
    Guid Id,
    string Title,
    IEnumerable<SimpleArtistDto> Artists,
    string? CoverUrl
);