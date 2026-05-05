using System;
using System.Collections.Generic;
using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumsAutocomplete;

public sealed record AlbumAutocompleteDto(
    Guid Id,
    string Title,
    IEnumerable<SimpleArtistDto> Artists, 
    string? CoverUrl
);