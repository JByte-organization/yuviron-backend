using System;
using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistById;

public record PlaylistDetailsClientDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    PlaylistVisibility Visibility,
    Guid? CreatorId,
    string CreatorName,
    bool IsEditorial,
    int TotalTracks,
    int TotalDurationMs,
    DateTime CreatedAt,
    DateTime UpdatedAt, 
    bool IsSaved = false
);