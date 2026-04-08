using System;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistsAutocomplete;

public sealed record ArtistAutocompleteDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    string? OwnerEmail 
);