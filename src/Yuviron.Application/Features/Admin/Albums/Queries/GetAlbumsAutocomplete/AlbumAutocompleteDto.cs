using System;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumsAutocomplete;

public sealed record AlbumAutocompleteDto(
    Guid Id,
    string Title,
    string ArtistNames,
    string? CoverUrl
);