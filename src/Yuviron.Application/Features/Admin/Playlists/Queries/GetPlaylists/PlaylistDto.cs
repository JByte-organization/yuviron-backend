using System;

namespace Yuviron.Application.Features.Admin.Playlists.Queries;

public sealed record PlaylistDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    bool IsPublic,
    bool IsEditorial,
    bool IsDeleted,
    DateTime CreatedAt,
    int TracksCount 
);