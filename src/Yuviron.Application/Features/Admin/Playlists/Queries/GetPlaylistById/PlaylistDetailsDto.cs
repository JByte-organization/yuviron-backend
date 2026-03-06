using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistById;

public sealed record PlaylistDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    bool IsPublic,
    bool IsEditorial,
    List<PlaylistTrackDto> Tracks
);

public sealed record PlaylistTrackDto(
    Guid TrackId,
    string Title,
    string? ArtistName,
    int Position
);